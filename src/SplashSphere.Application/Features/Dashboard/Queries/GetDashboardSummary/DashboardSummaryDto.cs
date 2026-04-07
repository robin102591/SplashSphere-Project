namespace SplashSphere.Application.Features.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// Tenant-wide or branch-scoped KPI snapshot for the admin dashboard.
/// All monetary values in Philippine Peso (₱), revenue counts only Completed transactions.
/// </summary>
public sealed record DashboardSummaryDto(
    // ── Revenue ───────────────────────────────────────────────────────────────
    decimal RevenueToday,
    decimal RevenueThisWeek,
    decimal RevenueThisMonth,

    // ── Transaction counts ────────────────────────────────────────────────────
    int TransactionsToday,
    int TransactionsThisWeek,
    int TransactionsThisMonth,

    // ── Queue (current state) ─────────────────────────────────────────────────
    int QueueWaiting,
    int QueueInService,

    // ── Workforce ─────────────────────────────────────────────────────────────
    int ActiveEmployees,
    int ClockedInToday,

    // ── Branch breakdown (null when filtered to a single branch) ──────────────
    IReadOnlyList<BranchKpiDto>? Branches,

    // ── Period comparison (percentage change) ─────────────────────────────────
    decimal? RevenueWeekChange = null,
    decimal? RevenueMonthChange = null,
    decimal? TransactionsWeekChange = null,
    decimal? TransactionsMonthChange = null);

public sealed record BranchKpiDto(
    string BranchId,
    string BranchName,
    decimal RevenueToday,
    int TransactionsToday,
    int QueueWaiting,
    int QueueInService);
