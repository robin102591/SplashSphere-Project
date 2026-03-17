using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Queue;

public sealed record QueueEntryDto(
    string Id,
    string BranchId,
    string BranchName,
    string QueueNumber,
    string PlateNumber,
    QueueStatus Status,
    QueuePriority Priority,
    string? CustomerId,
    string? CustomerFullName,
    string? CarId,
    string? TransactionId,
    int? EstimatedWaitMinutes,
    string? PreferredServices,
    string? Notes,
    DateTime? CalledAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    DateTime? NoShowAt,
    DateTime CreatedAt);

/// <summary>
/// Slimmed-down projection for the public wall-display.
/// Plate number is masked: first 3 chars + *** + last 2 chars (e.g. "ABC***34").
/// </summary>
public sealed record QueueDisplayEntryDto(
    string QueueNumber,
    string MaskedPlate,
    QueueStatus Status,
    QueuePriority Priority,
    int? EstimatedWaitMinutes);

/// <summary>
/// Full snapshot of the public queue display state, grouped by status.
/// Matches the <c>QueueDisplayUpdatedPayload</c> TypeScript type.
/// </summary>
public sealed record QueueDisplaySnapshotDto(
    string BranchId,
    IReadOnlyList<QueueDisplayEntryDto> Calling,
    IReadOnlyList<QueueDisplayEntryDto> InService,
    int WaitingCount);

public sealed record QueueStatsDto(
    int WaitingCount,
    int CalledCount,
    int InServiceCount,
    int ServedToday,
    double? AvgWaitMinutes);
