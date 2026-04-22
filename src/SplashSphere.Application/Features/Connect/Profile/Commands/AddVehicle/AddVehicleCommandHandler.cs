using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Profile.Commands.AddVehicle;

public sealed class AddVehicleCommandHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<AddVehicleCommand, Result<ConnectVehicleDto>>
{
    public async Task<Result<ConnectVehicleDto>> Handle(
        AddVehicleCommand request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated)
        {
            return Result.Failure<ConnectVehicleDto>(Error.Unauthorized("Sign in required."));
        }

        var userId = connectUser.ConnectUserId;

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

        var duplicate = await db.ConnectVehicles
            .AnyAsync(v => v.ConnectUserId == userId && v.PlateNumber == plate, cancellationToken);
        if (duplicate)
        {
            return Result.Failure<ConnectVehicleDto>(Error.Conflict(
                $"Plate '{plate}' is already on your profile."));
        }

        var vehicle = new ConnectVehicle(
            connectUserId: userId,
            makeId: make.Id,
            modelId: model.Id,
            plateNumber: plate,
            color: string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim(),
            year: request.Year);

        db.ConnectVehicles.Add(vehicle);

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
