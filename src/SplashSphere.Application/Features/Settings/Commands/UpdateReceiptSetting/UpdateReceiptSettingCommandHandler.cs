using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Subscription;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Settings.Commands.UpdateReceiptSetting;

public sealed class UpdateReceiptSettingCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IPlanEnforcementService planService)
    : IRequestHandler<UpdateReceiptSettingCommand, Result>
{
    public async Task<Result> Handle(
        UpdateReceiptSettingCommand request,
        CancellationToken cancellationToken)
    {
        // Per-branch overrides are an Enterprise feature. The tenant default
        // (BranchId = null) is always available. We can't gate this at the
        // endpoint level via RequiresFeatureAttribute because the gate only
        // applies when branchId is non-null on the same route.
        if (!string.IsNullOrWhiteSpace(request.BranchId))
        {
            var allowed = await planService.HasFeatureAsync(
                tenantContext.TenantId,
                FeatureKeys.BranchReceiptOverrides,
                cancellationToken);

            if (!allowed)
                return Result.Failure(Error.Forbidden(
                    "Per-branch receipt overrides require the Enterprise plan."));
        }

        // Find existing row by (tenant, branch). The unique partial indexes
        // on (TenantId, BranchId IS NULL) and (TenantId, BranchId IS NOT NULL)
        // guarantee at most one match.
        var setting = string.IsNullOrWhiteSpace(request.BranchId)
            ? await context.ReceiptSettings.FirstOrDefaultAsync(
                r => r.BranchId == null, cancellationToken)
            : await context.ReceiptSettings.FirstOrDefaultAsync(
                r => r.BranchId == request.BranchId, cancellationToken);

        // Upsert: create on first save (e.g. legacy tenants without an
        // onboarding-seeded row).
        if (setting is null)
        {
            setting = new ReceiptSetting(tenantContext.TenantId, request.BranchId);
            context.ReceiptSettings.Add(setting);
        }

        // ── Header ────────────────────────────────────────────────────────────
        setting.ShowLogo          = request.ShowLogo;
        setting.LogoSize          = request.LogoSize;
        setting.LogoPosition      = request.LogoPosition;
        setting.ShowBusinessName  = request.ShowBusinessName;
        setting.ShowTagline       = request.ShowTagline;
        setting.ShowBranchName    = request.ShowBranchName;
        setting.ShowBranchAddress = request.ShowBranchAddress;
        setting.ShowBranchContact = request.ShowBranchContact;
        setting.ShowTIN           = request.ShowTIN;
        setting.CustomHeaderText  = request.CustomHeaderText;

        // ── Body ──────────────────────────────────────────────────────────────
        setting.ShowServiceDuration   = request.ShowServiceDuration;
        setting.ShowEmployeeNames     = request.ShowEmployeeNames;
        setting.ShowVehicleInfo       = request.ShowVehicleInfo;
        setting.ShowDiscountBreakdown = request.ShowDiscountBreakdown;
        setting.ShowTaxLine           = request.ShowTaxLine;
        setting.ShowTransactionNumber = request.ShowTransactionNumber;
        setting.ShowDateTime          = request.ShowDateTime;
        setting.ShowCashierName       = request.ShowCashierName;

        // ── Customer ──────────────────────────────────────────────────────────
        setting.ShowCustomerName        = request.ShowCustomerName;
        setting.ShowCustomerPhone       = request.ShowCustomerPhone;
        setting.ShowLoyaltyPointsEarned = request.ShowLoyaltyPointsEarned;
        setting.ShowLoyaltyBalance      = request.ShowLoyaltyBalance;
        setting.ShowLoyaltyTier         = request.ShowLoyaltyTier;

        // ── Footer ────────────────────────────────────────────────────────────
        setting.ThankYouMessage  = request.ThankYouMessage;
        setting.PromoText        = request.PromoText;
        setting.ShowSocialMedia  = request.ShowSocialMedia;
        setting.ShowGCashQr      = request.ShowGCashQr;
        setting.ShowGCashNumber  = request.ShowGCashNumber;
        setting.CustomFooterText = request.CustomFooterText;

        // ── Format ────────────────────────────────────────────────────────────
        setting.ReceiptWidth = request.ReceiptWidth;
        setting.FontSize     = request.FontSize;
        setting.AutoCutPaper = request.AutoCutPaper;

        return Result.Success();
    }
}
