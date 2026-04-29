using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.PosStations.Queries.GetPosStationById;

public sealed class GetPosStationByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPosStationByIdQuery, PosStationDto>
{
    public async Task<PosStationDto> Handle(
        GetPosStationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var station = await context.PosStations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (station is null)
            throw new NotFoundException($"PosStation '{request.Id}' was not found.");

        return new PosStationDto(
            station.Id,
            station.BranchId,
            station.Name,
            station.IsActive,
            station.CreatedAt,
            station.UpdatedAt);
    }
}
