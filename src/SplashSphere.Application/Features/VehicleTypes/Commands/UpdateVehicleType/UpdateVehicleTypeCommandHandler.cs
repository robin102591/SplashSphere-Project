using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.VehicleTypes.Commands.UpdateVehicleType;

public sealed class UpdateVehicleTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateVehicleTypeCommand, Result>
{
    public async Task<Result> Handle(
        UpdateVehicleTypeCommand request,
        CancellationToken cancellationToken)
    {
        var vehicleType = await context.VehicleTypes
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (vehicleType is null)
            return Result.Failure(Error.NotFound("VehicleType", request.Id));

        var nameConflict = await context.VehicleTypes
            .AnyAsync(v => v.Name == request.Name && v.Id != request.Id, cancellationToken);

        if (nameConflict)
            return Result.Failure(Error.Conflict($"Vehicle type '{request.Name}' already exists."));

        vehicleType.Name = request.Name;

        return Result.Success();
    }
}
