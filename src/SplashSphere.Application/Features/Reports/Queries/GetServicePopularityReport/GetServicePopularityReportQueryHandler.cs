using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Reports.Queries.GetServicePopularityReport;

public sealed class GetServicePopularityReportQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetServicePopularityReportQuery, ServicePopularityReportDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<ServicePopularityReportDto> Handle(
        GetServicePopularityReportQuery request,
        CancellationToken cancellationToken)
    {
        var fromUtc = DateTime.SpecifyKind(request.From.ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var toUtc   = DateTime.SpecifyKind(request.To.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var top     = Math.Clamp(request.Top, 1, 100);

        // ── Branch name ───────────────────────────────────────────────────────
        string? branchName = null;
        if (request.BranchId is not null)
        {
            branchName = await context.Branches
                .AsNoTracking()
                .Where(b => b.Id == request.BranchId)
                .Select(b => b.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // ── Services — grouped from TransactionService lines ──────────────────
        var svcQuery = context.TransactionServices
            .AsNoTracking()
            .Where(ts =>
                ts.Transaction.Status == TransactionStatus.Completed &&
                ts.Transaction.CompletedAt >= fromUtc &&
                ts.Transaction.CompletedAt < toUtc);

        if (request.BranchId is not null)
            svcQuery = svcQuery.Where(ts => ts.Transaction.BranchId == request.BranchId);

        var services = await svcQuery
            .GroupBy(ts => new
            {
                ts.ServiceId,
                ServiceName  = ts.Service.Name,
                CategoryName = ts.Service.Category != null ? ts.Service.Category.Name : null,
            })
            .Select(g => new
            {
                g.Key.ServiceId,
                g.Key.ServiceName,
                g.Key.CategoryName,
                Count   = g.Count(),
                Revenue = g.Sum(ts => ts.UnitPrice),
            })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync(cancellationToken);

        var serviceItems = services
            .Select(s => new ServicePopularityItemDto(
                s.ServiceId,
                s.ServiceName,
                s.CategoryName,
                s.Count,
                s.Revenue,
                s.Count > 0 ? Math.Round(s.Revenue / s.Count, 2) : 0m))
            .ToList();

        // ── Packages — grouped from TransactionPackage lines ──────────────────
        var pkgQuery = context.TransactionPackages
            .AsNoTracking()
            .Where(tp =>
                tp.Transaction.Status == TransactionStatus.Completed &&
                tp.Transaction.CompletedAt >= fromUtc &&
                tp.Transaction.CompletedAt < toUtc);

        if (request.BranchId is not null)
            pkgQuery = pkgQuery.Where(tp => tp.Transaction.BranchId == request.BranchId);

        var packages = await pkgQuery
            .GroupBy(tp => new
            {
                tp.PackageId,
                PackageName = tp.Package.Name,
            })
            .Select(g => new
            {
                g.Key.PackageId,
                g.Key.PackageName,
                Count   = g.Count(),
                Revenue = g.Sum(tp => tp.UnitPrice),
            })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync(cancellationToken);

        var packageItems = packages
            .Select(p => new PackagePopularityItemDto(
                p.PackageId,
                p.PackageName,
                p.Count,
                p.Revenue,
                p.Count > 0 ? Math.Round(p.Revenue / p.Count, 2) : 0m))
            .ToList();

        return new ServicePopularityReportDto(
            request.From,
            request.To,
            request.BranchId,
            branchName,
            serviceItems,
            packageItems);
    }
}
