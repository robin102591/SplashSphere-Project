using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Cars.Queries.LookupCarByPlate;

public sealed class LookupCarByPlateQueryHandler(IApplicationDbContext context)
    : IRequestHandler<LookupCarByPlateQuery, CarDto?>
{
    public async Task<CarDto?> Handle(
        LookupCarByPlateQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedPlate = request.PlateNumber.ToUpperInvariant().Trim();

        return await context.Cars
            .AsNoTracking()
            .Where(c => c.PlateNumber == normalizedPlate)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
