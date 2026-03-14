using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.VehicleTypes.Commands.CreateVehicleType;

public sealed class CreateVehicleTypeCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateVehicleTypeCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateVehicleTypeCommand request,
        CancellationToken cancellationToken)
    {
        var nameExists = await context.VehicleTypes
            .AnyAsync(v => v.Name == request.Name, cancellationToken);

        if (nameExists)
            return Result.Failure<string>(Error.Conflict($"Vehicle type '{request.Name}' already exists."));

        var vehicleType = new VehicleType(tenantContext.TenantId, request.Name);
        context.VehicleTypes.Add(vehicleType);

        return Result.Success(vehicleType.Id);
    }
}
