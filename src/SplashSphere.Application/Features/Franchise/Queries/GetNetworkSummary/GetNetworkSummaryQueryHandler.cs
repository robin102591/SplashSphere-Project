using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Franchise.Queries.GetNetworkSummary;

public sealed class GetNetworkSummaryQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetNetworkSummaryQuery, NetworkSummaryDto>
{
    public async Task<NetworkSummaryDto> Handle(
        GetNetworkSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var franchisorId = tenantContext.TenantId;

        // Count franchisees by status
        var franchisees = await db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.ParentTenantId == franchisorId)
            .ToListAsync(cancellationToken);

        var totalFranchisees = franchisees.Count;
        var activeFranchisees = franchisees.Count(t => t.IsActive);

        // Count by agreement status
        var agreements = await db.FranchiseAgreements
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(a => a.FranchisorTenantId == franchisorId)
            .Select(a => a.Status)
            .ToListAsync(cancellationToken);

        var suspendedCount = agreements.Count(s => s == AgreementStatus.Suspended);
        var pendingCount = agreements.Count(s => s == AgreementStatus.Draft);

        // Royalty aggregation by status
        var royaltyAggregates = await db.RoyaltyPeriods
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(rp => rp.FranchisorTenantId == franchisorId)
            .GroupBy(rp => rp.Status)
            .Select(g => new { Status = g.Key, Total = g.Sum(rp => rp.TotalDue) })
            .ToListAsync(cancellationToken);

        var totalRoyaltiesCollected = royaltyAggregates
            .Where(a => a.Status == RoyaltyStatus.Paid)
            .Sum(a => a.Total);

        var pendingRoyalties = royaltyAggregates
            .Where(a => a.Status is RoyaltyStatus.Pending or RoyaltyStatus.Invoiced)
            .Sum(a => a.Total);

        var overdueRoyalties = royaltyAggregates
            .Where(a => a.Status == RoyaltyStatus.Overdue)
            .Sum(a => a.Total);

        // Network revenue this month from royalty periods
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var networkRevenueThisMonth = await db.RoyaltyPeriods
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(rp => rp.FranchisorTenantId == franchisorId
                         && rp.PeriodStart >= monthStart)
            .SumAsync(rp => rp.GrossRevenue, cancellationToken);

        var avgRevenue = totalFranchisees > 0
            ? Math.Round(networkRevenueThisMonth / totalFranchisees, 2, MidpointRounding.AwayFromZero)
            : 0m;

        return new NetworkSummaryDto(
            totalFranchisees,
            activeFranchisees,
            suspendedCount,
            pendingCount,
            networkRevenueThisMonth,
            totalRoyaltiesCollected,
            pendingRoyalties,
            overdueRoyalties,
            avgRevenue);
    }
}
