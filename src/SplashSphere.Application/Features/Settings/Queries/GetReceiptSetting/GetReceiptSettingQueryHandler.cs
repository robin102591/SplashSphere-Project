using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Application.Features.Settings.Queries.GetReceiptSetting;

public sealed class GetReceiptSettingQueryHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<GetReceiptSettingQuery, ReceiptSettingDto>
{
    public async Task<ReceiptSettingDto> Handle(
        GetReceiptSettingQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve branch-specific first (will be null in slice 2 since the
        // command doesn't accept a branchId yet — kept here for slice 4).
        ReceiptSetting? setting = null;

        if (!string.IsNullOrWhiteSpace(request.BranchId))
        {
            setting = await context.ReceiptSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.BranchId == request.BranchId,
                    cancellationToken);
        }

        // Fall back to tenant default (BranchId IS NULL).
        setting ??= await context.ReceiptSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.BranchId == null,
                cancellationToken);

        // Last-resort: synthesize defaults. Onboarding seeds one for every new
        // tenant, but old tenants from before slice 2 won't have one — return
        // an in-memory default until they hit Save.
        setting ??= new ReceiptSetting(tenantContext.TenantId);

        return ToDto(setting);
    }

    private static ReceiptSettingDto ToDto(ReceiptSetting r) => new(
        r.BranchId,
        r.ShowLogo,
        r.LogoSize,
        r.LogoPosition,
        r.ShowBusinessName,
        r.ShowTagline,
        r.ShowBranchName,
        r.ShowBranchAddress,
        r.ShowBranchContact,
        r.ShowTIN,
        r.CustomHeaderText,
        r.ShowServiceDuration,
        r.ShowEmployeeNames,
        r.ShowVehicleInfo,
        r.ShowDiscountBreakdown,
        r.ShowTaxLine,
        r.ShowTransactionNumber,
        r.ShowDateTime,
        r.ShowCashierName,
        r.ShowCustomerName,
        r.ShowCustomerPhone,
        r.ShowLoyaltyPointsEarned,
        r.ShowLoyaltyBalance,
        r.ShowLoyaltyTier,
        r.ThankYouMessage,
        r.PromoText,
        r.ShowSocialMedia,
        r.ShowGCashQr,
        r.ShowGCashNumber,
        r.CustomFooterText,
        r.ReceiptWidth,
        r.FontSize,
        r.AutoCutPaper);
}
