using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Services.Queries.GetServiceById;

public sealed class GetServiceByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetServiceByIdQuery, ServiceDetailDto>
{
    public async Task<ServiceDetailDto> Handle(
        GetServiceByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Three focused queries — avoids the cartesian JOIN that results from
        // a single query with both Pricing and Commissions collections included.
        var service = await context.Services
            .AsNoTracking()
            .Where(s => s.Id == request.Id)
            .Select(s => new
            {
                s.Id, s.Name, s.Description, s.BasePrice,
                s.CategoryId, CategoryName = s.Category.Name,
                s.IsActive, s.CreatedAt, s.UpdatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Service '{request.Id}' was not found.");

        var pricing = await context.ServicePricings
            .AsNoTracking()
            .Where(p => p.ServiceId == request.Id)
            .OrderBy(p => p.VehicleType.Name).ThenBy(p => p.Size.Name)
            .Select(p => new ServicePricingRowDto(
                p.Id,
                p.VehicleTypeId, p.VehicleType.Name,
                p.SizeId,        p.Size.Name,
                p.Price))
            .ToListAsync(cancellationToken);

        var commissions = await context.ServiceCommissions
            .AsNoTracking()
            .Where(c => c.ServiceId == request.Id)
            .OrderBy(c => c.VehicleType.Name).ThenBy(c => c.Size.Name)
            .Select(c => new ServiceCommissionRowDto(
                c.Id,
                c.VehicleTypeId, c.VehicleType.Name,
                c.SizeId,        c.Size.Name,
                c.Type,
                c.FixedAmount,
                c.PercentageRate))
            .ToListAsync(cancellationToken);

        return new ServiceDetailDto(
            service.Id,
            service.Name,
            service.Description,
            service.BasePrice,
            service.CategoryId,
            service.CategoryName,
            service.IsActive,
            service.CreatedAt,
            service.UpdatedAt,
            pricing,
            commissions);
    }
}
