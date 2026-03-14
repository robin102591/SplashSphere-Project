using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Sizes.Queries.GetSizes;

public sealed class GetSizesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSizesQuery, PagedResult<SizeDto>>
{
    public async Task<PagedResult<SizeDto>> Handle(
        GetSizesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Sizes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(s => s.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(s => s.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SizeDto(s.Id, s.Name, s.IsActive, s.CreatedAt, s.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<SizeDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
