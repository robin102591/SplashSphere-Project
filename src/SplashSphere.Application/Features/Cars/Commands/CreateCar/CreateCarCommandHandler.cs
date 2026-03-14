using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Cars.Commands.CreateCar;

public sealed class CreateCarCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateCarCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateCarCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedPlate = request.PlateNumber.ToUpperInvariant().Trim();

        var plateExists = await context.Cars
            .AnyAsync(c => c.PlateNumber == normalizedPlate, cancellationToken);

        if (plateExists)
            return Result.Failure<string>(Error.Conflict($"A car with plate number '{normalizedPlate}' already exists."));

        var vehicleTypeExists = await context.VehicleTypes
            .AnyAsync(v => v.Id == request.VehicleTypeId, cancellationToken);

        if (!vehicleTypeExists)
            return Result.Failure<string>(Error.Validation("Vehicle type ID is invalid."));

        var sizeExists = await context.Sizes
            .AnyAsync(s => s.Id == request.SizeId, cancellationToken);

        if (!sizeExists)
            return Result.Failure<string>(Error.Validation("Size ID is invalid."));

        if (!string.IsNullOrWhiteSpace(request.CustomerId))
        {
            var customerExists = await context.Customers
                .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (!customerExists)
                return Result.Failure<string>(Error.Validation("Customer ID is invalid."));
        }

        var car = new Car(
            tenantContext.TenantId,
            request.VehicleTypeId,
            request.SizeId,
            request.PlateNumber,
            request.CustomerId,
            request.MakeId,
            request.ModelId,
            request.Color,
            request.Year);

        car.Notes = request.Notes;

        context.Cars.Add(car);

        return Result.Success(car.Id);
    }
}
