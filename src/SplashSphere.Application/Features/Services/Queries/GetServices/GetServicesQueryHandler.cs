using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Queries.GetServices;

public sealed class GetServicesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetServicesQuery, PagedResult<ServiceSummaryDto>>
{
    public async Task<PagedResult<ServiceSummaryDto>> Handle(
        GetServicesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Services.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.CategoryId))
            query = query.Where(s => s.CategoryId == request.CategoryId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(s => s.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(s => s.Category.Name)
            .ThenBy(s => s.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new ServiceSummaryDto(
                s.Id,
                s.Name,
                s.Description,
                s.BasePrice,
                s.CategoryId,
                s.Category.Name,
                s.IsActive,
                s.CreatedAt,
                s.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<ServiceSummaryDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
