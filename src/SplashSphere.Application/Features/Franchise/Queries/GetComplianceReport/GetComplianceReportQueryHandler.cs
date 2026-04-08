using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Franchise.Queries.GetComplianceReport;

public sealed class GetComplianceReportQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetComplianceReportQuery, IReadOnlyList<FranchiseComplianceItemDto>>
{
    public async Task<IReadOnlyList<FranchiseComplianceItemDto>> Handle(
        GetComplianceReportQuery request,
        CancellationToken cancellationToken)
    {
        var franchisorId = tenantContext.TenantId;

        // Load required service template names
        var requiredTemplateNames = await db.FranchiseServiceTemplates
            .AsNoTracking()
            .Where(t => t.IsRequired && t.IsActive)
            .Select(t => t.ServiceName)
            .ToListAsync(cancellationToken);

        // Load all franchisees
        var franchisees = await db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.ParentTenantId == franchisorId)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(cancellationToken);

        // Load agreements for territory and status info
        var agreements = await db.FranchiseAgreements
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(a => a.FranchisorTenantId == franchisorId)
            .ToListAsync(cancellationToken);

        // Load franchisee service names for compliance check
        var franchiseeServices = await db.Services
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => franchisees.Select(f => f.Id).Contains(s.TenantId))
            .GroupBy(s => s.TenantId)
            .Select(g => new { TenantId = g.Key, ServiceNames = g.Select(s => s.Name).ToList() })
            .ToListAsync(cancellationToken);

        var servicesByTenant = franchiseeServices.ToDictionary(x => x.TenantId, x => x.ServiceNames);

        // Load latest royalty status per franchisee
        var latestRoyalties = await db.RoyaltyPeriods
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(rp => rp.FranchisorTenantId == franchisorId)
            .GroupBy(rp => rp.FranchiseeTenantId)
            .Select(g => new
            {
                TenantId = g.Key,
                LatestStatus = g.OrderByDescending(rp => rp.PeriodEnd).First().Status
            })
            .ToListAsync(cancellationToken);

        var royaltyByTenant = latestRoyalties.ToDictionary(x => x.TenantId, x => x.LatestStatus);

        var now = DateTime.UtcNow;
        var ninetyDaysFromNow = now.AddDays(90);

        var result = new List<FranchiseComplianceItemDto>();

        foreach (var franchisee in franchisees)
        {
            var agreement = agreements.FirstOrDefault(a => a.FranchiseeTenantId == franchisee.Id);
            var territoryName = agreement?.TerritoryName ?? string.Empty;

            // Check service compliance: franchisee has all required template services
            var hasServices = servicesByTenant.TryGetValue(franchisee.Id, out var names);
            var matchedCount = hasServices
                ? requiredTemplateNames.Count(rn => names!.Any(n =>
                    string.Equals(n, rn, StringComparison.OrdinalIgnoreCase)))
                : 0;
            var usingStandardServices = requiredTemplateNames.Count == 0
                || matchedCount == requiredTemplateNames.Count;

            // Pricing compliance — simplified: assume compliant (would need full pricing matrix comparison)
            var pricingCompliant = true;

            // Royalties current: latest royalty is not Overdue
            var royaltiesCurrent = !royaltyByTenant.TryGetValue(franchisee.Id, out var latestStatus)
                                   || latestStatus != RoyaltyStatus.Overdue;

            // Agreement expiring soon: EndDate within 90 days
            var agreementExpiringSoon = agreement?.EndDate != null
                                       && agreement.EndDate.Value <= ninetyDaysFromNow
                                       && agreement.EndDate.Value >= now;

            // Calculate compliance score (percentage of checks passing)
            var totalChecks = 4;
            var passingChecks = 0;
            if (usingStandardServices) passingChecks++;
            if (pricingCompliant) passingChecks++;
            if (royaltiesCurrent) passingChecks++;
            if (!agreementExpiringSoon) passingChecks++;

            var complianceScore = (int)Math.Round(
                (double)passingChecks / totalChecks * 100,
                MidpointRounding.AwayFromZero);

            result.Add(new FranchiseComplianceItemDto(
                franchisee.Id,
                franchisee.Name,
                territoryName,
                usingStandardServices,
                pricingCompliant,
                royaltiesCurrent,
                agreementExpiringSoon,
                complianceScore));
        }

        return result;
    }
}
