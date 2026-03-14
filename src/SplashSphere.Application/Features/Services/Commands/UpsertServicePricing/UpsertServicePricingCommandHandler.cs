using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Commands.UpsertServicePricing;

/// <summary>
/// Atomically replaces the pricing matrix for a service.
/// Uses an explicit DB transaction because <c>BulkUpsertAsync</c> calls
/// <c>ExecuteDeleteAsync</c> which bypasses EF change tracking and hits the
/// database immediately — the delete and subsequent insert must share the same
/// transaction to avoid a window where both are absent.
/// </summary>
public sealed class UpsertServicePricingCommandHandler(
    IApplicationDbContext context,
    IServicePricingRepository pricingRepo,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpsertServicePricingCommand, Result>
{
    public async Task<Result> Handle(
        UpsertServicePricingCommand request,
        CancellationToken cancellationToken)
    {
        var serviceExists = await context.Services
            .AnyAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (!serviceExists)
            return Result.Failure(Error.NotFound("Service", request.ServiceId));

        var rows = request.Rows
            .Select(r => new ServicePricing(
                tenantContext.TenantId,
                request.ServiceId,
                r.VehicleTypeId,
                r.SizeId,
                r.Price))
            .ToList();

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await pricingRepo.BulkUpsertAsync(request.ServiceId, rows, cancellationToken);
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
