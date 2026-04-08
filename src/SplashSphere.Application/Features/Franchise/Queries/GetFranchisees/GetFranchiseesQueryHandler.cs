using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Queries.GetFranchisees;

public sealed class GetFranchiseesQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetFranchiseesQuery, PagedResult<FranchiseeListItemDto>>
{
    public async Task<PagedResult<FranchiseeListItemDto>> Handle(
        GetFranchiseesQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.ParentTenantId == tenantContext.TenantId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(t => t.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(t => t.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new FranchiseeListItemDto(
                t.Id,
                t.Name,
                t.FranchiseCode,
                db.FranchiseAgreements
                    .IgnoreQueryFilters()
                    .Where(a => a.FranchiseeTenantId == t.Id && a.FranchisorTenantId == tenantContext.TenantId)
                    .Select(a => a.TerritoryName)
                    .FirstOrDefault() ?? string.Empty,
                t.Branches.Count,
                t.IsActive,
                db.FranchiseAgreements
                    .IgnoreQueryFilters()
                    .Where(a => a.FranchiseeTenantId == t.Id && a.FranchisorTenantId == tenantContext.TenantId)
                    .Select(a => a.Status)
                    .FirstOrDefault(),
                0m, // RevenueThisMonth — will come from royalty periods
                0m  // RoyaltyDue — will come from royalty periods
            ))
            .ToListAsync(cancellationToken);

        return PagedResult<FranchiseeListItemDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
