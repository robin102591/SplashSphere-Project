using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Packages.Commands.UpsertPackageCommission;

/// <summary>
/// Atomically replaces the commission matrix for a package.
/// See <see cref="UpsertPackagePricingCommandHandler"/> for the explicit-transaction rationale.
/// </summary>
public sealed class UpsertPackageCommissionCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpsertPackageCommissionCommand, Result>
{
    public async Task<Result> Handle(
        UpsertPackageCommissionCommand request,
        CancellationToken cancellationToken)
    {
        var packageExists = await context.ServicePackages
            .AnyAsync(p => p.Id == request.PackageId, cancellationToken);

        if (!packageExists)
            return Result.Failure(Error.NotFound("Package", request.PackageId));

        var rows = request.Rows
            .Select(r => new PackageCommission(
                tenantContext.TenantId,
                request.PackageId,
                r.VehicleTypeId,
                r.SizeId,
                r.PercentageRate))
            .ToList();

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await context.PackageCommissions
                .Where(c => c.PackageId == request.PackageId)
                .ExecuteDeleteAsync(cancellationToken);

            await context.PackageCommissions.AddRangeAsync(rows, cancellationToken);
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
