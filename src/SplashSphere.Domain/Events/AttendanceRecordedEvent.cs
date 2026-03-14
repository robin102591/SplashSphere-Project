namespace SplashSphere.Domain.Events;

/// <summary>
/// Raised after an attendance record is created (clock-in) or updated (clock-out).
/// Consumed by: SignalR hub (broadcasts <c>AttendanceUpdated</c> to branch group)
/// so the POS attendance screen reflects real-time status.
/// </summary>
public sealed record AttendanceRecordedEvent(
    string AttendanceId,
    string TenantId,
    string BranchId,
    string EmployeeId,
    string EmployeeFullName,
    DateOnly Date,
    DateTime TimeIn,
    /// <summary>True on clock-in, false on clock-out.</summary>
    bool IsClockIn,
    DateTime? TimeOut = null
) : DomainEventBase;
