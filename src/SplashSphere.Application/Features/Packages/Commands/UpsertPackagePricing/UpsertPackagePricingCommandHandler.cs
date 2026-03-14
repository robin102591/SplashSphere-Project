using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Packages.Commands.UpsertPackagePricing;

/// <summary>
/// Atomically replaces the pricing matrix for a package.
/// Uses an explicit DB transaction because ExecuteDeleteAsync bypasses EF change tracking
/// and hits the database immediately — both the delete and insert must share a transaction.
/// </summary>
public sealed class UpsertPackagePricingCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpsertPackagePricingCommand, Result>
{
    public async Task<Result> Handle(
        UpsertPackagePricingCommand request,
        CancellationToken cancellationToken)
    {
        var packageExists = await context.ServicePackages
            .AnyAsync(p => p.Id == request.PackageId, cancellationToken);

        if (!packageExists)
            return Result.Failure(Error.NotFound("Package", request.PackageId));

        var rows = request.Rows
            .Select(r => new PackagePricing(
                tenantContext.TenantId,
                request.PackageId,
                r.VehicleTypeId,
                r.SizeId,
                r.Price))
            .ToList();

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await context.PackagePricings
                .Where(p => p.PackageId == request.PackageId)
                .ExecuteDeleteAsync(cancellationToken);

            await context.PackagePricings.AddRangeAsync(rows, cancellationToken);
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
