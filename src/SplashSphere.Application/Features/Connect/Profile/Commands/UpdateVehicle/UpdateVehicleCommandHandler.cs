using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Profile.Commands.UpdateVehicle;

public sealed class UpdateVehicleCommandHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<UpdateVehicleCommand, Result<ConnectVehicleDto>>
{
    public async Task<Result<ConnectVehicleDto>> Handle(
        UpdateVehicleCommand request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated)
        {
            return Result.Failure<ConnectVehicleDto>(Error.Unauthorized("Sign in required."));
        }

        var userId = connectUser.ConnectUserId;

        var vehicle = await db.ConnectVehicles
            .FirstOrDefaultAsync(
                v => v.Id == request.VehicleId && v.ConnectUserId == userId,
                cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure<ConnectVehicleDto>(Error.NotFound("ConnectVehicle", request.VehicleId));
        }

        var make = await db.GlobalMakes
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MakeId && m.IsActive, cancellationToken);
        if (make is null)
        {
            return Result.Failure<ConnectVehicleDto>(Error.NotFound("GlobalMake", request.MakeId));
        }

        var model = await db.GlobalModels
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.Id == request.ModelId && m.GlobalMakeId == make.Id && m.IsActive,
                cancellationToken);
        if (model is null)
        {
            return Result.Failure<ConnectVehicleDto>(Error.Validation(
                "GlobalModel does not belong to the selected make or is inactive."));
        }

        var plate = request.PlateNumber.ToUpperInvariant().Trim();

        if (!string.Equals(plate, vehicle.PlateNumber, StringComparison.Ordinal))
        {
            var duplicate = await db.ConnectVehicles
                .AnyAsync(
                    v => v.ConnectUserId == userId
                        && v.PlateNumber == plate
                        && v.Id != vehicle.Id,
                    cancellationToken);
            if (duplicate)
            {
                return Result.Failure<ConnectVehicleDto>(Error.Conflict(
                    $"Plate '{plate}' is already on your profile."));
            }
        }

        vehicle.MakeId = make.Id;
        vehicle.ModelId = model.Id;
        vehicle.PlateNumber = plate;
        vehicle.Color = string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim();
        vehicle.Year = request.Year;

        // UoWBehavior saves.
        return Result.Success(new ConnectVehicleDto(
            vehicle.Id,
            make.Id,
            make.Name,
            model.Id,
            model.Name,
            vehicle.PlateNumber,
            vehicle.Color,
            vehicle.Year));
    }
}
