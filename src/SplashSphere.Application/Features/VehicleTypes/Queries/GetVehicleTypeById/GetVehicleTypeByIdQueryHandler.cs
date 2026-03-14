using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.VehicleTypes.Queries.GetVehicleTypeById;

public sealed class GetVehicleTypeByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetVehicleTypeByIdQuery, VehicleTypeDto>
{
    public async Task<VehicleTypeDto> Handle(
        GetVehicleTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var v = await context.VehicleTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Vehicle type '{request.Id}' was not found.");

        return new VehicleTypeDto(v.Id, v.Name, v.IsActive, v.CreatedAt, v.UpdatedAt);
    }
}
