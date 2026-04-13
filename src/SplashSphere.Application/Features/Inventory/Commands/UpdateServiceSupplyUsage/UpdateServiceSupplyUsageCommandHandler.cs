using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.UpdateServiceSupplyUsage;

public sealed class UpdateServiceSupplyUsageCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<UpdateServiceSupplyUsageCommand, Result>
{
    public async Task<Result> Handle(
        UpdateServiceSupplyUsageCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await db.ServiceSupplyUsages
            .Where(u => u.ServiceId == request.ServiceId)
            .ToListAsync(cancellationToken);

        db.ServiceSupplyUsages.RemoveRange(existing);

        foreach (var entry in request.Usages)
        {
            var usage = new ServiceSupplyUsage(
                tenantContext.TenantId,
                request.ServiceId,
                entry.SupplyItemId,
                entry.SizeId,
                entry.QuantityPerUse);

            db.ServiceSupplyUsages.Add(usage);
        }

        return Result.Success();
    }
}
