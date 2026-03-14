using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Packages.Commands.UpdatePackage;

public sealed class UpdatePackageCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<UpdatePackageCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePackageCommand request,
        CancellationToken cancellationToken)
    {
        var package = await context.ServicePackages
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (package is null)
            return Result.Failure(Error.NotFound("Package", request.Id));

        var nameConflict = await context.ServicePackages
            .AnyAsync(p => p.Name == request.Name && p.Id != request.Id, cancellationToken);

        if (nameConflict)
            return Result.Failure(Error.Conflict($"Package '{request.Name}' already exists."));

        var distinctServiceIds = request.ServiceIds.Distinct().ToList();

        var foundCount = await context.Services
            .CountAsync(s => distinctServiceIds.Contains(s.Id), cancellationToken);

        if (foundCount != distinctServiceIds.Count)
            return Result.Failure(Error.Validation("One or more service IDs are invalid."));

        // Update scalar fields.
        package.Name        = request.Name;
        package.Description = request.Description;

        // Replace service list via EF tracked changes — no explicit transaction needed.
        // Both the package update and the service list replacement are flushed atomically
        // by UnitOfWorkBehavior's single SaveChangesAsync call.
        var existingLinks = await context.PackageServices
            .Where(ps => ps.PackageId == request.Id)
            .ToListAsync(cancellationToken);

        context.PackageServices.RemoveRange(existingLinks);

        foreach (var serviceId in distinctServiceIds)
            context.PackageServices.Add(new PackageService(tenantContext.TenantId, request.Id, serviceId));

        return Result.Success();
    }
}
