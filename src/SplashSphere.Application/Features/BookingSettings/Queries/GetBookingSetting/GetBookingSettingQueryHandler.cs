using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.BookingSettings.Queries.GetBookingSetting;

public sealed class GetBookingSettingQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetBookingSettingQuery, BookingSettingDto>
{
    public async Task<BookingSettingDto> Handle(
        GetBookingSettingQuery request,
        CancellationToken cancellationToken)
    {
        var row = await db.BookingSettings
            .AsNoTracking()
            .Where(s => s.TenantId == tenantContext.TenantId
                     && s.BranchId == request.BranchId)
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            // Lazy upsert — return entity defaults without creating a row.
            return new BookingSettingDto(
                BranchId: request.BranchId,
                OpenTime: new TimeOnly(8, 0),
                CloseTime: new TimeOnly(18, 0),
                SlotIntervalMinutes: 30,
                MaxBookingsPerSlot: 3,
                AdvanceBookingDays: 7,
                MinLeadTimeMinutes: 120,
                NoShowGraceMinutes: 15,
                IsBookingEnabled: true,
                ShowInPublicDirectory: true);
        }

        return new BookingSettingDto(
            row.BranchId,
            row.OpenTime,
            row.CloseTime,
            row.SlotIntervalMinutes,
            row.MaxBookingsPerSlot,
            row.AdvanceBookingDays,
            row.MinLeadTimeMinutes,
            row.NoShowGraceMinutes,
            row.IsBookingEnabled,
            row.ShowInPublicDirectory);
    }
}
