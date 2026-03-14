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
/// Payload sent to clients on the <c>QueueDisplayUpdated</c> SignalR event.
/// Broadcast to <c>queue-display:{branchId}</c> (public, no auth).
/// Plate number is masked to protect vehicle owner privacy.
/// </summary>
public sealed record QueueDisplayUpdatedPayload(
    string QueueEntryId,
    string BranchId,
    string QueueNumber,
    string MaskedPlate,
    string Status,
    string Priority,
    int? EstimatedWaitMinutes);
