using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.MerchandiseCategories.Queries.GetMerchandiseCategories;

public sealed class GetMerchandiseCategoriesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMerchandiseCategoriesQuery, PagedResult<MerchandiseCategoryDto>>
{
    public async Task<PagedResult<MerchandiseCategoryDto>> Handle(
        GetMerchandiseCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.MerchandiseCategories.AsNoTracking();

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
            .Select(c => new MerchandiseCategoryDto(
                c.Id, c.Name, c.Description, c.IsActive, c.CreatedAt, c.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<MerchandiseCategoryDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
