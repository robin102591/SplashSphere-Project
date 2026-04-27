using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Connect.Queue;

/// <summary>
/// The caller's current "in flight" queue entry, if any. Returned by
/// <c>GET /api/v1/connect/queue/active</c>.
/// <para>
/// Exposes the physical queue position (number of WAITING entries ahead plus
/// the called entry) so the Connect app can render a live "you are 3rd in line"
/// indicator. An entry counts as in-flight while it is in one of the
/// non-terminal states <c>Waiting</c>, <c>Called</c>, or <c>InService</c>.
/// </para>
/// </summary>
public sealed record ConnectActiveQueueDto(
    string QueueEntryId,
    string TenantId,
    string TenantName,
    string BranchId,
    string BranchName,
    string QueueNumber,
    QueueStatus Status,
    QueuePriority Priority,
    int? AheadCount,
    int? EstimatedWaitMinutes,
    DateTime? CalledAt,
    DateTime? StartedAt,
    string? BookingId);
