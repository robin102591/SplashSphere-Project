using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Commands.CreateFranchiseAgreement;

public sealed class CreateFranchiseAgreementCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<CreateFranchiseAgreementCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateFranchiseAgreementCommand request,
        CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure<string>(Error.NotFound("Tenant", tenantContext.TenantId));

        if (tenant.TenantType != TenantType.Franchisor)
            return Result.Failure<string>(Error.Forbidden("Only Franchisor tenants can create franchise agreements."));

        // Verify franchisee tenant exists and belongs to this franchisor
        var franchisee = await db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                t => t.Id == request.FranchiseeTenantId
                     && t.TenantType == TenantType.Franchisee
                     && t.ParentTenantId == tenantContext.TenantId,
                cancellationToken);

        if (franchisee is null)
            return Result.Failure<string>(Error.NotFound("Franchisee tenant", request.FranchiseeTenantId));

        // Check no duplicate active agreement for this franchisee
        var existingAgreement = await db.FranchiseAgreements
            .IgnoreQueryFilters()
            .AnyAsync(
                a => a.FranchisorTenantId == tenantContext.TenantId
                     && a.FranchiseeTenantId == request.FranchiseeTenantId
                     && a.Status == AgreementStatus.Active,
                cancellationToken);

        if (existingAgreement)
            return Result.Failure<string>(Error.Conflict("An active agreement already exists for this franchisee."));

        var agreement = new FranchiseAgreement(tenantContext.TenantId, request.FranchiseeTenantId, request.TerritoryName)
        {
            AgreementNumber = request.AgreementNumber,
            TerritoryDescription = request.TerritoryDescription,
            ExclusiveTerritory = request.ExclusiveTerritory,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            InitialFranchiseFee = request.InitialFranchiseFee,
            CustomRoyaltyRate = request.CustomRoyaltyRate,
            CustomMarketingFeeRate = request.CustomMarketingFeeRate,
            Notes = request.Notes,
            Status = AgreementStatus.Active,
        };

        db.FranchiseAgreements.Add(agreement);
        return Result.Success(agreement.Id);
    }
}
