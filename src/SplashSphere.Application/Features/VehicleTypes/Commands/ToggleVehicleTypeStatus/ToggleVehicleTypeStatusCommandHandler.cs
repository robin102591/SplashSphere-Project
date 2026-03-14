using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.VehicleTypes.Commands.ToggleVehicleTypeStatus;

public sealed class ToggleVehicleTypeStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleVehicleTypeStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleVehicleTypeStatusCommand request,
        CancellationToken cancellationToken)
    {
        var vehicleType = await context.VehicleTypes
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (vehicleType is null)
            return Result.Failure(Error.NotFound("VehicleType", request.Id));

        vehicleType.IsActive = !vehicleType.IsActive;

        return Result.Success();
    }
}
