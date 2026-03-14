using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Packages.Queries.GetPackageById;

public sealed class GetPackageByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPackageByIdQuery, PackageDetailDto>
{
    public async Task<PackageDetailDto> Handle(
        GetPackageByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Three focused queries — avoids the cartesian JOIN that results from
        // a single query with both Services, Pricing, and Commissions collections included.
        var package = await context.ServicePackages
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new
            {
                p.Id, p.Name, p.Description,
                p.IsActive, p.CreatedAt, p.UpdatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Package '{request.Id}' was not found.");

        var services = await context.PackageServices
            .AsNoTracking()
            .Where(ps => ps.PackageId == request.Id)
            .OrderBy(ps => ps.Service.Category.Name)
            .ThenBy(ps => ps.Service.Name)
            .Select(ps => new PackageServiceDto(
                ps.ServiceId,
                ps.Service.Name,
                ps.Service.Category.Name))
            .ToListAsync(cancellationToken);

        var pricing = await context.PackagePricings
            .AsNoTracking()
            .Where(p => p.PackageId == request.Id)
            .OrderBy(p => p.VehicleType.Name).ThenBy(p => p.Size.Name)
            .Select(p => new PackagePricingRowDto(
                p.Id,
                p.VehicleTypeId, p.VehicleType.Name,
                p.SizeId,        p.Size.Name,
                p.Price))
            .ToListAsync(cancellationToken);

        var commissions = await context.PackageCommissions
            .AsNoTracking()
            .Where(c => c.PackageId == request.Id)
            .OrderBy(c => c.VehicleType.Name).ThenBy(c => c.Size.Name)
            .Select(c => new PackageCommissionRowDto(
                c.Id,
                c.VehicleTypeId, c.VehicleType.Name,
                c.SizeId,        c.Size.Name,
                c.PercentageRate))
            .ToListAsync(cancellationToken);

        return new PackageDetailDto(
            package.Id,
            package.Name,
            package.Description,
            package.IsActive,
            package.CreatedAt,
            package.UpdatedAt,
            services,
            pricing,
            commissions);
    }
}
