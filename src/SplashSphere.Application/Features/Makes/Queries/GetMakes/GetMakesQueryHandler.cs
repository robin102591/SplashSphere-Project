using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Makes.Queries.GetMakes;

public sealed class GetMakesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMakesQuery, PagedResult<MakeDto>>
{
    public async Task<PagedResult<MakeDto>> Handle(
        GetMakesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Makes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(m => m.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(m => m.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MakeDto(m.Id, m.Name, m.IsActive, m.CreatedAt, m.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<MakeDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
