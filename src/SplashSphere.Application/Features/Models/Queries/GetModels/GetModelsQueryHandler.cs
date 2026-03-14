using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Models.Queries.GetModels;

public sealed class GetModelsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetModelsQuery, PagedResult<ModelDto>>
{
    public async Task<PagedResult<ModelDto>> Handle(
        GetModelsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Models.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.MakeId))
            query = query.Where(m => m.MakeId == request.MakeId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(m => m.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(m => m.Make.Name)
            .ThenBy(m => m.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new ModelDto(m.Id, m.MakeId, m.Make.Name, m.Name, m.IsActive, m.CreatedAt, m.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<ModelDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
