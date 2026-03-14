using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Cars.Queries.GetCars;

public sealed class GetCarsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCarsQuery, PagedResult<CarDto>>
{
    public async Task<PagedResult<CarDto>> Handle(
        GetCarsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Cars.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.CustomerId))
            query = query.Where(c => c.CustomerId == request.CustomerId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(c => c.PlateNumber.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.PlateNumber)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CarDto(
                c.Id,
                c.PlateNumber,
                c.VehicleTypeId,
                c.VehicleType.Name,
                c.SizeId,
                c.Size.Name,
                c.MakeId,
                c.Make != null ? c.Make.Name : null,
                c.ModelId,
                c.Model != null ? c.Model.Name : null,
                c.CustomerId,
                c.Customer != null ? c.Customer.FirstName + " " + c.Customer.LastName : null,
                c.Color,
                c.Year,
                c.Notes,
                c.CreatedAt,
                c.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<CarDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
