using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Application.Features.Settings.Queries.GetDisplaySetting;

public sealed class GetDisplaySettingQueryHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<GetDisplaySettingQuery, DisplaySettingDto>
{
    public async Task<DisplaySettingDto> Handle(
        GetDisplaySettingQuery request,
        CancellationToken cancellationToken)
    {
        DisplaySetting? setting = null;

        if (!string.IsNullOrWhiteSpace(request.BranchId))
        {
            setting = await context.DisplaySettings
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.BranchId == request.BranchId, cancellationToken);
        }

        setting ??= await context.DisplaySettings
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.BranchId == null, cancellationToken);

        // Last-resort: in-memory defaults (legacy tenants from before this slice
        // landed). The first save will create the row.
        setting ??= new DisplaySetting(tenantContext.TenantId);

        return ToDto(setting);
    }

    private static DisplaySettingDto ToDto(DisplaySetting d) => new(
        d.BranchId,
        d.ShowLogo,
        d.ShowBusinessName,
        d.ShowTagline,
        d.ShowDateTime,
        d.ShowGCashQr,
        d.ShowSocialMedia,
        d.PromoMessages,
        d.PromoRotationSeconds,
        d.ShowVehicleInfo,
        d.ShowCustomerName,
        d.ShowLoyaltyTier,
        d.ShowDiscountBreakdown,
        d.ShowTaxLine,
        d.ShowPaymentMethod,
        d.ShowChangeAmount,
        d.ShowPointsEarned,
        d.ShowPointsBalance,
        d.ShowThankYouMessage,
        d.ShowPromoText,
        d.CompletionHoldSeconds,
        d.Theme,
        d.FontSize,
        d.Orientation);
}
