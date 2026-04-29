using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Display.DTOs;

namespace SplashSphere.Application.Features.Display.Queries.GetCurrentDisplayTransaction;

/// <summary>
/// Returns the in-progress transaction for a station (Pending / InProgress)
/// formatted as a display-safe payload, or <c>null</c> if no transaction is
/// active. Called by the customer-display device when SignalR reconnects so
/// it can rehydrate the Building screen instead of stalling on Idle.
/// </summary>
public sealed record GetCurrentDisplayTransactionQuery(
    string BranchId,
    string StationId) : IQuery<DisplayCurrentResultDto>;

/// <summary>
/// Wrapper that includes the active transaction (if any). The frontend treats
/// <c>Transaction == null</c> as "stay on Idle".
/// </summary>
public sealed record DisplayCurrentResultDto(DisplayTransactionResultDto? Transaction);
