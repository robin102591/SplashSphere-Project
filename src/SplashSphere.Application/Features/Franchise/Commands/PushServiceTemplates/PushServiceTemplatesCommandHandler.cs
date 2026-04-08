using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Commands.PushServiceTemplates;

public sealed class PushServiceTemplatesCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<PushServiceTemplatesCommand, Result>
{
    public async Task<Result> Handle(
        PushServiceTemplatesCommand request,
        CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure(Error.NotFound("Tenant", tenantContext.TenantId));

        if (tenant.TenantType != TenantType.Franchisor)
            return Result.Failure(Error.Forbidden("Only Franchisor tenants can push service templates."));

        // Verify franchisee exists and belongs to this franchisor
        var franchiseeExists = await db.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(
                t => t.Id == request.FranchiseeTenantId
                     && t.TenantType == TenantType.Franchisee
                     && t.ParentTenantId == tenantContext.TenantId,
                cancellationToken);

        if (!franchiseeExists)
            return Result.Failure(Error.NotFound("Franchisee tenant", request.FranchiseeTenantId));

        // Load all templates for this franchisor (query filter scopes automatically)
        var templates = await db.FranchiseServiceTemplates
            .Where(t => t.IsActive)
            .ToListAsync(cancellationToken);

        // Load franchisee's existing services
        var franchiseeServices = await db.Services
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == request.FranchiseeTenantId)
            .ToListAsync(cancellationToken);

        foreach (var template in templates)
        {
            var existingService = franchiseeServices
                .FirstOrDefault(s => s.Name == template.ServiceName);

            if (existingService is not null)
            {
                // Update base price to match template
                existingService.BasePrice = template.BasePrice;
            }
            else
            {
                // Create new service for franchisee — use first available category or skip
                var categoryId = await db.Services
                    .IgnoreQueryFilters()
                    .Where(s => s.TenantId == request.FranchiseeTenantId)
                    .Select(s => s.CategoryId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (categoryId is null)
                    continue; // Franchisee has no categories yet — skip creating this service

                var newService = new Service(
                    request.FranchiseeTenantId,
                    categoryId,
                    template.ServiceName,
                    template.BasePrice,
                    template.Description);

                db.Services.Add(newService);
            }
        }

        return Result.Success();
    }
}
