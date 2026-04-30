using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.PosStations.Queries.GetPosStations;

public sealed class GetPosStationsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPosStationsQuery, IReadOnlyList<PosStationDto>>
{
    public async Task<IReadOnlyList<PosStationDto>> Handle(
        GetPosStationsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.PosStations
            .AsNoTracking()
            .Where(s => s.BranchId == request.BranchId)
            .OrderBy(s => s.Name)
            .Select(s => new PosStationDto(
                s.Id,
                s.BranchId,
                s.Name,
                s.IsActive,
                s.CreatedAt,
                s.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
