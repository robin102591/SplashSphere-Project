using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.PosStations.Commands.UpdatePosStation;

public sealed class UpdatePosStationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdatePosStationCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePosStationCommand request,
        CancellationToken cancellationToken)
    {
        var station = await context.PosStations
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (station is null)
            return Result.Failure(Error.NotFound("PosStation", request.Id));

        var nameConflict = await context.PosStations.AnyAsync(
            s => s.BranchId == station.BranchId
              && s.Name == request.Name
              && s.Id != request.Id,
            cancellationToken);

        if (nameConflict)
            return Result.Failure(
                Error.Conflict($"A station named '{request.Name}' already exists in this branch."));

        station.Name = request.Name;
        station.IsActive = request.IsActive;

        return Result.Success();
    }
}
