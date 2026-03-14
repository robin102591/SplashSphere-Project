using Microsoft.AspNetCore.SignalR;

namespace SplashSphere.Infrastructure.Hubs;

/// <summary>
/// Central SignalR hub for real-time POS and queue events.
/// <para>
/// Group conventions:
/// <list type="bullet">
///   <item><c>tenant:{tenantId}</c> — all connections for a tenant (admin dashboard).</item>
///   <item><c>tenant:{tenantId}:branch:{branchId}</c> — branch-scoped POS events.</item>
///   <item><c>queue-display:{branchId}</c> — public wall-TV queue display (no auth).</item>
/// </list>
/// </para>
/// Clients call the Join* methods once they connect.
/// The server pushes events via <c>IHubContext&lt;NotificationHub&gt;</c>
/// from domain event handlers.
/// </summary>
public sealed class NotificationHub : Hub
{
    /// <summary>Subscribe to all events for a tenant (admin dashboard).</summary>
    public async Task JoinTenantGroup(string tenantId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant:{tenantId}");

    /// <summary>Subscribe to branch-level POS events.</summary>
    public async Task JoinBranchGroup(string tenantId, string branchId)
        => await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"tenant:{tenantId}:branch:{branchId}");

    /// <summary>Subscribe to the public queue display feed (no auth required).</summary>
    public async Task JoinQueueDisplay(string branchId)
        => await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"queue-display:{branchId}");

    /// <summary>Leave a group explicitly (optional — automatic on disconnect).</summary>
    public async Task LeaveGroup(string groupName)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
}
