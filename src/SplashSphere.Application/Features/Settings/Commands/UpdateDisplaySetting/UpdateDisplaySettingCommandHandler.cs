using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Subscription;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Settings.Commands.UpdateDisplaySetting;

public sealed class UpdateDisplaySettingCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IPlanEnforcementService planService)
    : IRequestHandler<UpdateDisplaySettingCommand, Result>
{
    public async Task<Result> Handle(
        UpdateDisplaySettingCommand request,
        CancellationToken cancellationToken)
    {
        // Per-branch overrides require Enterprise. Tenant default (BranchId = null)
        // is always available. The gate lives here, not at the endpoint, because
        // the same route serves both paths and the gate depends on the payload.
        if (!string.IsNullOrWhiteSpace(request.BranchId))
        {
            var allowed = await planService.HasFeatureAsync(
                tenantContext.TenantId,
                FeatureKeys.BranchDisplayOverrides,
                cancellationToken);

            if (!allowed)
                return Result.Failure(Error.Forbidden(
                    "Per-branch display overrides require the Enterprise plan."));
        }

        // ── Plan gate: promo message cap ────────────────────────────────────
        // Starter = 1, Growth = 5, Enterprise = 20. The validator enforces the
        // hard 20 ceiling globally; this enforces the per-plan limit on top.
        var plan = await planService.GetActivePlanAsync(tenantContext.TenantId, cancellationToken);
        if (request.PromoMessages.Count > plan.MaxPromoMessages)
        {
            return Result.Failure(Error.Validation(
                $"Your {plan.Name} plan allows up to {plan.MaxPromoMessages} promo message(s). " +
                "Upgrade for more, or remove some entries."));
        }

        var setting = string.IsNullOrWhiteSpace(request.BranchId)
            ? await context.DisplaySettings.FirstOrDefaultAsync(
                d => d.BranchId == null, cancellationToken)
            : await context.DisplaySettings.FirstOrDefaultAsync(
                d => d.BranchId == request.BranchId, cancellationToken);

        if (setting is null)
        {
            setting = new DisplaySetting(tenantContext.TenantId, request.BranchId);
            context.DisplaySettings.Add(setting);
        }

        // ── Idle ─────────────────────────────────────────────────────────────
        setting.ShowLogo             = request.ShowLogo;
        setting.ShowBusinessName     = request.ShowBusinessName;
        setting.ShowTagline          = request.ShowTagline;
        setting.ShowDateTime         = request.ShowDateTime;
        setting.ShowGCashQr          = request.ShowGCashQr;
        setting.ShowSocialMedia      = request.ShowSocialMedia;
        setting.PromoMessages        = request.PromoMessages.ToList();
        setting.PromoRotationSeconds = request.PromoRotationSeconds;

        // ── Building / transaction ───────────────────────────────────────────
        setting.ShowVehicleInfo       = request.ShowVehicleInfo;
        setting.ShowCustomerName      = request.ShowCustomerName;
        setting.ShowLoyaltyTier       = request.ShowLoyaltyTier;
        setting.ShowDiscountBreakdown = request.ShowDiscountBreakdown;
        setting.ShowTaxLine           = request.ShowTaxLine;

        // ── Completion ───────────────────────────────────────────────────────
        setting.ShowPaymentMethod     = request.ShowPaymentMethod;
        setting.ShowChangeAmount      = request.ShowChangeAmount;
        setting.ShowPointsEarned      = request.ShowPointsEarned;
        setting.ShowPointsBalance     = request.ShowPointsBalance;
        setting.ShowThankYouMessage   = request.ShowThankYouMessage;
        setting.ShowPromoText         = request.ShowPromoText;
        setting.CompletionHoldSeconds = request.CompletionHoldSeconds;

        // ── Appearance ───────────────────────────────────────────────────────
        setting.Theme       = request.Theme;
        setting.FontSize    = request.FontSize;
        setting.Orientation = request.Orientation;

        return Result.Success();
    }
}
