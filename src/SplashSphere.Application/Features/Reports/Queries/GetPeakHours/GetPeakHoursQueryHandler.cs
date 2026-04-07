using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Reports.Queries.GetPeakHours;

public sealed class GetPeakHoursQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetPeakHoursQuery, PeakHoursDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    private static readonly string[] DayNames =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    public async Task<PeakHoursDto> Handle(
        GetPeakHoursQuery request,
        CancellationToken cancellationToken)
    {
        var fromUtc = DateTime.SpecifyKind(request.From.ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(request.To.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);

        var txQuery = context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == TransactionStatus.Completed &&
                t.CompletedAt >= fromUtc &&
                t.CompletedAt < toUtc);

        if (request.BranchId is not null)
            txQuery = txQuery.Where(t => t.BranchId == request.BranchId);

        // Load minimal fields and group in-memory (Manila TZ arithmetic not translatable).
        var txRows = await txQuery
            .Select(t => new { t.CompletedAt, t.FinalAmount })
            .ToListAsync(cancellationToken);

        // Group by (DayOfWeek, Hour) in Manila time.
        var grouped = txRows
            .Select(t =>
            {
                var manila = t.CompletedAt!.Value + ManilaOffset;
                return new { DayOfWeek = (int)manila.DayOfWeek, Hour = manila.Hour, t.FinalAmount };
            })
            .GroupBy(t => (t.DayOfWeek, t.Hour))
            .Select(g => new HourlySlotDto(
                g.Key.DayOfWeek,
                g.Key.Hour,
                g.Count(),
                g.Sum(t => t.FinalAmount)))
            .ToList();

        // Fill in empty slots so frontend gets a complete 7×24 grid.
        var existingKeys = grouped.Select(s => (s.DayOfWeek, s.Hour)).ToHashSet();
        for (var d = 0; d < 7; d++)
        for (var h = 0; h < 24; h++)
        {
            if (!existingKeys.Contains((d, h)))
                grouped.Add(new HourlySlotDto(d, h, 0, 0m));
        }

        var slots = grouped.OrderBy(s => s.DayOfWeek).ThenBy(s => s.Hour).ToList();

        // Determine peaks.
        var peakSlot = slots.MaxBy(s => s.TransactionCount);
        var peakDay = peakSlot is not null ? DayNames[peakSlot.DayOfWeek] : "N/A";
        var peakHour = peakSlot?.Hour ?? 0;

        return new PeakHoursDto(
            request.From,
            request.To,
            request.BranchId,
            txRows.Count,
            peakDay,
            peakHour,
            slots);
    }
}
