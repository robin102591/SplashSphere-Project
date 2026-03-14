using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.VehicleTypes.Queries.GetVehicleTypes;

public sealed class GetVehicleTypesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetVehicleTypesQuery, PagedResult<VehicleTypeDto>>
{
    public async Task<PagedResult<VehicleTypeDto>> Handle(
        GetVehicleTypesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.VehicleTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(v => v.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(v => v.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(v => new VehicleTypeDto(v.Id, v.Name, v.IsActive, v.CreatedAt, v.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<VehicleTypeDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
