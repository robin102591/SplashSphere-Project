using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Packages.Commands.CreatePackage;

public sealed class CreatePackageCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreatePackageCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreatePackageCommand request,
        CancellationToken cancellationToken)
    {
        var nameExists = await context.ServicePackages
            .AnyAsync(p => p.Name == request.Name, cancellationToken);

        if (nameExists)
            return Result.Failure<string>(Error.Conflict($"Package '{request.Name}' already exists."));

        var distinctServiceIds = request.ServiceIds.Distinct().ToList();

        var foundCount = await context.Services
            .CountAsync(s => distinctServiceIds.Contains(s.Id), cancellationToken);

        if (foundCount != distinctServiceIds.Count)
            return Result.Failure<string>(Error.Validation("One or more service IDs are invalid."));

        var package = new ServicePackage(tenantContext.TenantId, request.Name, request.Description);
        context.ServicePackages.Add(package);

        foreach (var serviceId in distinctServiceIds)
            context.PackageServices.Add(new PackageService(tenantContext.TenantId, package.Id, serviceId));

        // UnitOfWorkBehavior saves package + all PackageService join rows atomically.
        return Result.Success(package.Id);
    }
}
