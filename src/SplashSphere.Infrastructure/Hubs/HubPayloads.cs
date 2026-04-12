namespace SplashSphere.Infrastructure.Hubs;

/// <summary>
/// Payload sent to clients on the <c>TransactionUpdated</c> SignalR event.
/// Broadcast to <c>tenant:{tenantId}:branch:{branchId}</c> whenever a
/// transaction is created or its status changes.
/// </summary>
public sealed record TransactionUpdatedPayload(
    string TransactionId,
    string BranchId,
    string TransactionNumber,
    string Status,
    decimal FinalAmount);

/// <summary>
/// Payload sent to clients on the <c>DashboardMetricsUpdated</c> SignalR event.
/// Broadcast to <c>tenant:{tenantId}</c> when revenue or headcount KPIs change.
/// Clients should re-fetch <c>GET /api/v1/dashboard/summary</c> on receipt.
/// </summary>
public sealed record DashboardMetricsUpdatedPayload(
    string TenantId,
    string BranchId);

/// <summary>
/// Payload sent to clients on the <c>AttendanceUpdated</c> SignalR event.
/// Broadcast to <c>tenant:{tenantId}:branch:{branchId}</c> on clock-in or clock-out.
/// </summary>
public sealed record AttendanceUpdatedPayload(
    string AttendanceId,
    string EmployeeId,
    string EmployeeFullName,
    string BranchId,
    DateOnly Date,
    bool IsClockIn,
    DateTime TimeIn,
    DateTime? TimeOut);

/// <summary>
/// Payload sent to clients on the <c>QueueUpdated</c> SignalR event.
/// Broadcast to <c>tenant:{tenantId}:branch:{branchId}</c> on any queue state change.
/// </summary>
public sealed record QueueUpdatedPayload(
    string QueueEntryId,
    string BranchId,
    string QueueNumber,
    string PlateNumber,
    string Status,
    string Priority,
    int? EstimatedWaitMinutes);

/// <summary>
/// Entry included in <see cref="QueueDisplayUpdatedPayload"/>.
/// Plate number is pre-masked to protect vehicle owner privacy.
/// </summary>
public sealed record QueueDisplayEntryPayload(
    string QueueNumber,
    string MaskedPlate,
    int Status,
    int Priority,
    int? EstimatedWaitMinutes);

/// <summary>
/// Full snapshot sent to <c>queue-display:{branchId}</c> (public, no auth) on any queue change.
/// Contains all Called entries (shown in "Now Calling"), all InService entries, and waiting count.
/// Matches the <c>QueueDisplayUpdatedPayload</c> TypeScript type.
/// </summary>
public sealed record QueueDisplayUpdatedPayload(
    string BranchId,
    IReadOnlyList<QueueDisplayEntryPayload> Calling,
    IReadOnlyList<QueueDisplayEntryPayload> InService,
    int WaitingCount);

/// <summary>
/// Payload sent to clients on the <c>NotificationReceived</c> SignalR event.
/// Broadcast to <c>tenant:{tenantId}</c> when a new persistent notification is created.
/// </summary>
public sealed record NotificationReceivedPayload(
    string Id,
    int Type,
    int Category,
    int Severity,
    string Title,
    string Message,
    string? ReferenceId,
    string? ReferenceType,
    string? ActionUrl,
    string? ActionLabel,
    DateTime CreatedAt);

/// <summary>
/// Payload sent to clients on the <c>LowStockAlert</c> SignalR event.
/// Broadcast to <c>tenant:{tenantId}</c> when daily stock check finds low items.
/// </summary>
public sealed record LowStockAlertPayload(
    string MerchandiseId,
    string MerchandiseName,
    string Sku,
    int CurrentStock,
    int LowStockThreshold);
