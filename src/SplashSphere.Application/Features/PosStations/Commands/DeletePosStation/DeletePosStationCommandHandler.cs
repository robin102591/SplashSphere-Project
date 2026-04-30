using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.PosStations.Commands.DeletePosStation;

public sealed class DeletePosStationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeletePosStationCommand, Result>
{
    public async Task<Result> Handle(
        DeletePosStationCommand request,
        CancellationToken cancellationToken)
    {
        var station = await context.PosStations
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (station is null)
            return Result.Failure(Error.NotFound("PosStation", request.Id));

        context.PosStations.Remove(station);
        return Result.Success();
    }
}
