using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Connect.Booking.Queries.GetAvailableSlots;

public sealed class GetAvailableSlotsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<BookingSlotDto>>
{
    private static readonly TimeZoneInfo Manila =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

    private static readonly BookingStatus[] CountableStatuses =
    [
        BookingStatus.Confirmed,
        BookingStatus.Arrived,
        BookingStatus.InService,
    ];

    public async Task<IReadOnlyList<BookingSlotDto>> Handle(
        GetAvailableSlotsQuery request,
        CancellationToken cancellationToken)
    {
        var setting = await db.BookingSettings
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                s => s.TenantId == request.TenantId && s.BranchId == request.BranchId,
                cancellationToken);

        if (setting is null || !setting.IsBookingEnabled) return [];

        var nowUtc = DateTime.UtcNow;
        var nowManila = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, Manila);
        var todayManila = DateOnly.FromDateTime(nowManila);

        // Outside the advance booking window or in the past → no slots.
        var maxDate = todayManila.AddDays(setting.AdvanceBookingDays);
        if (request.Date < todayManila || request.Date > maxDate) return [];

        // Enumerate slots from OpenTime to CloseTime.
        var slots = new List<(DateTime LocalStart, DateTime LocalEnd)>();
        var cursor = request.Date.ToDateTime(setting.OpenTime);
        var closeLocal = request.Date.ToDateTime(setting.CloseTime);
        while (cursor.AddMinutes(setting.SlotIntervalMinutes) <= closeLocal)
        {
            slots.Add((cursor, cursor.AddMinutes(setting.SlotIntervalMinutes)));
            cursor = cursor.AddMinutes(setting.SlotIntervalMinutes);
        }
        if (slots.Count == 0) return [];

        var firstSlotUtc = TimeZoneInfo.ConvertTimeToUtc(slots[0].LocalStart, Manila);
        var lastSlotEndUtc = TimeZoneInfo.ConvertTimeToUtc(slots[^1].LocalEnd, Manila);

        // Count existing bookings overlapping the day's slot window.
        var existing = await db.Bookings
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(b => b.TenantId == request.TenantId
                     && b.BranchId == request.BranchId
                     && CountableStatuses.Contains(b.Status)
                     && b.SlotStart >= firstSlotUtc
                     && b.SlotStart < lastSlotEndUtc)
            .Select(b => b.SlotStart)
            .ToListAsync(cancellationToken);

        var counts = existing
            .GroupBy(s => s)
            .ToDictionary(g => g.Key, g => g.Count());

        var minLeadUtc = nowUtc.AddMinutes(setting.MinLeadTimeMinutes);

        return slots.Select(s =>
        {
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(s.LocalStart, Manila);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(s.LocalEnd, Manila);
            var used = counts.TryGetValue(startUtc, out var c) ? c : 0;
            var remaining = Math.Max(0, setting.MaxBookingsPerSlot - used);
            return new BookingSlotDto(
                startUtc,
                endUtc,
                s.LocalStart.ToString("HH:mm"),
                startUtc < minLeadUtc ? 0 : remaining);
        })
        .Where(slot => slot.RemainingCapacity > 0)
        .ToList();
    }
}
