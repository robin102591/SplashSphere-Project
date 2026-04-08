using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Franchise.Queries.GetBenchmarks;

public sealed class GetBenchmarksQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetBenchmarksQuery, IReadOnlyList<FranchiseBenchmarkDto>>
{
    public async Task<IReadOnlyList<FranchiseBenchmarkDto>> Handle(
        GetBenchmarksQuery request,
        CancellationToken cancellationToken)
    {
        // Load current tenant to find parent franchisor
        var currentTenant = await db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.Id == tenantContext.TenantId)
            .Select(t => new { t.Id, t.ParentTenantId })
            .FirstOrDefaultAsync(cancellationToken);

        if (currentTenant?.ParentTenantId is null)
            return [];

        // Load all sibling franchisees (same parent)
        var siblings = await db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.ParentTenantId == currentTenant.ParentTenantId)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        if (siblings.Count == 0)
            return [];

        var totalInNetwork = siblings.Count;

        // Current month boundaries
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        // Compute this month's revenue per franchisee from completed transactions
        var revenueByTenant = await db.Transactions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => siblings.Contains(t.TenantId)
                        && t.Status == TransactionStatus.Completed
                        && t.CreatedAt >= monthStart
                        && t.CreatedAt < monthEnd)
            .GroupBy(t => t.TenantId)
            .Select(g => new { TenantId = g.Key, Revenue = g.Sum(t => t.FinalAmount) })
            .ToListAsync(cancellationToken);

        var revenueMap = revenueByTenant.ToDictionary(x => x.TenantId, x => x.Revenue);

        // Branch count per franchisee
        var branchCountByTenant = await db.Branches
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(b => siblings.Contains(b.TenantId))
            .GroupBy(b => b.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var branchMap = branchCountByTenant.ToDictionary(x => x.TenantId, x => x.Count);

        // Transaction count per franchisee this month
        var txCountByTenant = await db.Transactions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => siblings.Contains(t.TenantId)
                        && t.Status == TransactionStatus.Completed
                        && t.CreatedAt >= monthStart
                        && t.CreatedAt < monthEnd)
            .GroupBy(t => t.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var txMap = txCountByTenant.ToDictionary(x => x.TenantId, x => x.Count);

        var benchmarks = new List<FranchiseBenchmarkDto>();

        // Monthly Revenue benchmark
        var myRevenue = revenueMap.GetValueOrDefault(tenantContext.TenantId, 0m);
        var allRevenues = siblings.Select(id => revenueMap.GetValueOrDefault(id, 0m)).OrderDescending().ToList();
        var avgRevenue = allRevenues.Count > 0
            ? Math.Round(allRevenues.Average(), 2, MidpointRounding.AwayFromZero)
            : 0m;
        var revenueRank = allRevenues.IndexOf(myRevenue) + 1;
        benchmarks.Add(new FranchiseBenchmarkDto("Monthly Revenue", myRevenue, avgRevenue, revenueRank, totalInNetwork));

        // Branch Count benchmark
        var myBranches = (decimal)branchMap.GetValueOrDefault(tenantContext.TenantId, 0);
        var allBranches = siblings.Select(id => (decimal)branchMap.GetValueOrDefault(id, 0)).OrderDescending().ToList();
        var avgBranches = allBranches.Count > 0
            ? Math.Round(allBranches.Average(), 2, MidpointRounding.AwayFromZero)
            : 0m;
        var branchRank = allBranches.IndexOf(myBranches) + 1;
        benchmarks.Add(new FranchiseBenchmarkDto("Branch Count", myBranches, avgBranches, branchRank, totalInNetwork));

        // Transaction Count benchmark
        var myTxCount = (decimal)txMap.GetValueOrDefault(tenantContext.TenantId, 0);
        var allTxCounts = siblings.Select(id => (decimal)txMap.GetValueOrDefault(id, 0)).OrderDescending().ToList();
        var avgTxCount = allTxCounts.Count > 0
            ? Math.Round(allTxCounts.Average(), 2, MidpointRounding.AwayFromZero)
            : 0m;
        var txRank = allTxCounts.IndexOf(myTxCount) + 1;
        benchmarks.Add(new FranchiseBenchmarkDto("Transaction Count", myTxCount, avgTxCount, txRank, totalInNetwork));

        return benchmarks;
    }
}
