using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.PosStations.Commands.CreatePosStation;

public sealed class CreatePosStationCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreatePosStationCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreatePosStationCommand request,
        CancellationToken cancellationToken)
    {
        var branchExists = await context.Branches
            .AnyAsync(b => b.Id == request.BranchId, cancellationToken);

        if (!branchExists)
            return Result.Failure<string>(Error.NotFound("Branch", request.BranchId));

        var nameTaken = await context.PosStations
            .AnyAsync(s => s.BranchId == request.BranchId && s.Name == request.Name, cancellationToken);

        if (nameTaken)
            return Result.Failure<string>(
                Error.Conflict($"A station named '{request.Name}' already exists in this branch."));

        var station = new PosStation(tenantContext.TenantId, request.BranchId, request.Name);
        context.PosStations.Add(station);

        return Result.Success(station.Id);
    }
}
