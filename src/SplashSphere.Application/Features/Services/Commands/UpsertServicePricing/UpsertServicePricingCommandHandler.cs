using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Commands.UpsertServicePricing;

/// <summary>
/// Atomically replaces the pricing matrix for a service.
/// Uses an explicit DB transaction because <c>ExecuteDeleteAsync</c> bypasses
/// EF change tracking and hits the database immediately — the delete and the
/// subsequent insert must share the same transaction to avoid a window where
/// both are absent.
/// </summary>
public sealed class UpsertServicePricingCommandHandler(
    IApplicationDbContext context,
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
            // EF Core 7+ applies the global tenant filter to ExecuteDeleteAsync.
            await context.ServicePricings
                .Where(sp => sp.ServiceId == request.ServiceId)
                .ExecuteDeleteAsync(cancellationToken);

            await context.ServicePricings.AddRangeAsync(rows, cancellationToken);

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
