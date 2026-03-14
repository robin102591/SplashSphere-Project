using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Cars.Commands.UpdateCar;

public sealed class UpdateCarCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCarCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCarCommand request,
        CancellationToken cancellationToken)
    {
        var car = await context.Cars
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (car is null)
            return Result.Failure(Error.NotFound("Car", request.Id));

        var vehicleTypeExists = await context.VehicleTypes
            .AnyAsync(v => v.Id == request.VehicleTypeId, cancellationToken);

        if (!vehicleTypeExists)
            return Result.Failure(Error.Validation("Vehicle type ID is invalid."));

        var sizeExists = await context.Sizes
            .AnyAsync(s => s.Id == request.SizeId, cancellationToken);

        if (!sizeExists)
            return Result.Failure(Error.Validation("Size ID is invalid."));

        car.VehicleTypeId = request.VehicleTypeId;
        car.SizeId        = request.SizeId;
        car.MakeId        = request.MakeId;
        car.ModelId       = request.ModelId;
        car.Color         = request.Color;
        car.Year          = request.Year;
        car.Notes         = request.Notes;

        return Result.Success();
    }
}
