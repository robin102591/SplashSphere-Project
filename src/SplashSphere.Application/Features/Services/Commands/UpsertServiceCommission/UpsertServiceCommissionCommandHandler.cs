using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Commands.UpsertServiceCommission;

/// <summary>
/// Atomically replaces the commission matrix for a service.
/// See <see cref="UpsertServicePricingCommandHandler"/> for the explicit-transaction rationale.
/// </summary>
public sealed class UpsertServiceCommissionCommandHandler(
    IApplicationDbContext context,
    IServiceCommissionRepository commissionRepo,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpsertServiceCommissionCommand, Result>
{
    public async Task<Result> Handle(
        UpsertServiceCommissionCommand request,
        CancellationToken cancellationToken)
    {
        var serviceExists = await context.Services
            .AnyAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (!serviceExists)
            return Result.Failure(Error.NotFound("Service", request.ServiceId));

        var rows = request.Rows
            .Select(r => new ServiceCommission(
                tenantContext.TenantId,
                request.ServiceId,
                r.VehicleTypeId,
                r.SizeId,
                r.Type,
                r.FixedAmount,
                r.PercentageRate))
            .ToList();

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await commissionRepo.BulkUpsertAsync(request.ServiceId, rows, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return Result.Success();
    }
}
