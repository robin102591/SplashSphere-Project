using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Shifts.Queries.GetShiftSettings;

public sealed class GetShiftSettingsQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetShiftSettingsQuery, ShiftSettingsDto>
{
    public async Task<ShiftSettingsDto> Handle(
        GetShiftSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await db.ShiftSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantContext.TenantId, cancellationToken);

        // Return defaults if no settings record exists yet
        return settings is null
            ? new ShiftSettingsDto(2000m, 50m, 200m, true, new TimeOnly(20, 0))
            : new ShiftSettingsDto(
                settings.DefaultOpeningFund,
                settings.AutoApproveThreshold,
                settings.FlagThreshold,
                settings.RequireShiftForTransactions,
                settings.EndOfDayReminderTime);
    }
}
