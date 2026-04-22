using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Profile.Commands.RemoveVehicle;

public sealed class RemoveVehicleCommandHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<RemoveVehicleCommand, Result>
{
    public async Task<Result> Handle(
        RemoveVehicleCommand request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated)
        {
            return Result.Failure(Error.Unauthorized("Sign in required."));
        }

        var userId = connectUser.ConnectUserId;

        var vehicle = await db.ConnectVehicles
            .FirstOrDefaultAsync(
                v => v.Id == request.VehicleId && v.ConnectUserId == userId,
                cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure(Error.NotFound("ConnectVehicle", request.VehicleId));
        }

        db.ConnectVehicles.Remove(vehicle);

        // UoWBehavior saves.
        return Result.Success();
    }
}
