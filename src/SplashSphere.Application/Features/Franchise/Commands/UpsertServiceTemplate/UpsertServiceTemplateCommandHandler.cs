using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Commands.UpsertServiceTemplate;

public sealed class UpsertServiceTemplateCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<UpsertServiceTemplateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        UpsertServiceTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure<string>(Error.NotFound("Tenant", tenantContext.TenantId));

        if (tenant.TenantType != TenantType.Franchisor)
            return Result.Failure<string>(Error.Forbidden("Only Franchisor tenants can manage service templates."));

        if (!string.IsNullOrWhiteSpace(request.Id))
        {
            // Update existing template (query filter scopes to current franchisor)
            var existing = await db.FranchiseServiceTemplates
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (existing is null)
                return Result.Failure<string>(Error.NotFound("FranchiseServiceTemplate", request.Id));

            existing.ServiceName = request.ServiceName;
            existing.Description = request.Description;
            existing.CategoryName = request.CategoryName;
            existing.BasePrice = request.BasePrice;
            existing.DurationMinutes = request.DurationMinutes;
            existing.IsRequired = request.IsRequired;
            existing.PricingMatrixJson = request.PricingMatrixJson;
            existing.CommissionMatrixJson = request.CommissionMatrixJson;

            return Result.Success(existing.Id);
        }

        // Create new template
        var template = new FranchiseServiceTemplate(tenantContext.TenantId, request.ServiceName, request.BasePrice)
        {
            Description = request.Description,
            CategoryName = request.CategoryName,
            DurationMinutes = request.DurationMinutes,
            IsRequired = request.IsRequired,
            PricingMatrixJson = request.PricingMatrixJson,
            CommissionMatrixJson = request.CommissionMatrixJson,
        };

        db.FranchiseServiceTemplates.Add(template);
        return Result.Success(template.Id);
    }
}
