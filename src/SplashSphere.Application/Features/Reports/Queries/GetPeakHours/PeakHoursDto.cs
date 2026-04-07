namespace SplashSphere.Application.Features.Reports.Queries.GetPeakHours;

public sealed record PeakHoursDto(
    DateOnly From,
    DateOnly To,
    string? BranchId,
    int TotalTransactions,
    string PeakDay,
    int PeakHour,
    IReadOnlyList<HourlySlotDto> Slots);

/// <summary>
/// One cell in the heatmap grid: day-of-week × hour-of-day.
/// </summary>
/// <param name="DayOfWeek">0 = Sunday … 6 = Saturday.</param>
/// <param name="Hour">0–23 (Manila time).</param>
public sealed record HourlySlotDto(
    int DayOfWeek,
    int Hour,
    int TransactionCount,
    decimal Revenue);
