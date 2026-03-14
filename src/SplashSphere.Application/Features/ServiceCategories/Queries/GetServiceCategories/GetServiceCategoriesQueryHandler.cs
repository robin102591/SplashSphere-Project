using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.ServiceCategories.Queries.GetServiceCategories;

public sealed class GetServiceCategoriesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetServiceCategoriesQuery, PagedResult<ServiceCategoryDto>>
{
    public async Task<PagedResult<ServiceCategoryDto>> Handle(
        GetServiceCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ServiceCategories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(c => c.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ServiceCategoryDto(
                c.Id, c.Name, c.Description, c.IsActive, c.CreatedAt, c.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<ServiceCategoryDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
