using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Display.DTOs;

namespace SplashSphere.Application.Features.Display.Queries.GetCurrentDisplayTransaction;

public sealed class GetCurrentDisplayTransactionQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetCurrentDisplayTransactionQuery, DisplayCurrentResultDto>
{
    public async Task<DisplayCurrentResultDto> Handle(
        GetCurrentDisplayTransactionQuery request,
        CancellationToken cancellationToken)
    {
        var dto = await DisplayTransactionLoader.LoadByStationAsync(
            db,
            request.BranchId,
            request.StationId,
            cancellationToken);

        return new DisplayCurrentResultDto(dto);
    }
}
