using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Packages.Queries.GetPackages;

public sealed class GetPackagesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPackagesQuery, PagedResult<PackageSummaryDto>>
{
    public async Task<PagedResult<PackageSummaryDto>> Handle(
        GetPackagesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ServicePackages.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(p => p.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PackageSummaryDto(
                p.Id,
                p.Name,
                p.Description,
                p.PackageServices.Count,
                p.IsActive,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<PackageSummaryDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
