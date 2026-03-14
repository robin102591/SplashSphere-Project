using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Cars.Queries.GetCarById;

public sealed class GetCarByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCarByIdQuery, CarDto>
{
    public async Task<CarDto> Handle(
        GetCarByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Cars
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Car '{request.Id}' was not found.");
    }
}
