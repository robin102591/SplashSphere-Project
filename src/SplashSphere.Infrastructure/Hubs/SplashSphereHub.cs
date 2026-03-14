using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SplashSphere.Infrastructure.Hubs;

/// <summary>
/// Central SignalR hub for real-time POS and queue events.
/// <para>
/// <b>Authentication:</b> the hub itself does NOT require auth so that anonymous
/// clients (public queue-display wall TVs) can establish a connection.
/// Individual authenticated methods are protected with <see cref="AuthorizeAttribute"/>.
/// </para>
/// <para>
/// <b>Group conventions:</b>
/// <list type="bullet">
///   <item><c>tenant:{tenantId}</c> — tenant-wide admin dashboard events.
///   Joined automatically on connect for authenticated users.</item>
///   <item><c>tenant:{tenantId}:branch:{branchId}</c> — branch-scoped POS events.
///   Joined explicitly by calling <see cref="JoinBranch"/>.</item>
///   <item><c>queue-display:{branchId}</c> — public wall-TV display, no auth.</item>
/// </list>
/// </para>
/// <para>
/// <b>Client events pushed by the server:</b>
/// <c>TransactionUpdated</c>, <c>DashboardMetricsUpdated</c>,
/// <c>AttendanceUpdated</c>, <c>QueueUpdated</c>, <c>QueueDisplayUpdated</c>.
/// </para>
/// </summary>
public sealed class SplashSphereHub : Hub
{
    // ── Group name helpers (used by hub and notification handlers) ────────────

    public static string TenantGroup(string tenantId)
        => $"tenant:{tenantId}";

    public static string BranchGroup(string tenantId, string branchId)
        => $"tenant:{tenantId}:branch:{branchId}";

    public static string QueueDisplayGroup(string branchId)
        => $"queue-display:{branchId}";

    // ── Connection lifecycle ──────────────────────────────────────────────────

    /// <summary>
    /// Authenticated users are automatically added to their tenant group on connect
    /// so tenant-wide events (<c>DashboardMetricsUpdated</c>) are received immediately
    /// without requiring an explicit join call.
    /// Anonymous connections (queue display) pass through without any group assignment.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        if (Context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantId = Context.User.FindFirst("org_id")?.Value;
            if (!string.IsNullOrEmpty(tenantId))
                await Groups.AddToGroupAsync(Context.ConnectionId, TenantGroup(tenantId));
        }

        await base.OnConnectedAsync();
    }

    // ── Authenticated methods ─────────────────────────────────────────────────

    /// <summary>
    /// Subscribe to branch-level POS events (<c>TransactionUpdated</c>,
    /// <c>QueueUpdated</c>, <c>AttendanceUpdated</c>).
    /// The <c>tenantId</c> is sourced from the JWT <c>org_id</c> claim — never
    /// from client input — to prevent cross-tenant group injection.
    /// </summary>
    [Authorize]
    public async Task JoinBranch(string branchId)
    {
        var tenantId = Context.User!.FindFirst("org_id")?.Value
            ?? throw new HubException("No tenant context in token.");

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            BranchGroup(tenantId, branchId));
    }

    /// <summary>
    /// Unsubscribe from a branch group (e.g. when switching branches in the POS).
    /// </summary>
    [Authorize]
    public async Task LeaveBranch(string branchId)
    {
        var tenantId = Context.User!.FindFirst("org_id")?.Value
            ?? throw new HubException("No tenant context in token.");

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            BranchGroup(tenantId, branchId));
    }

    // ── Anonymous method ──────────────────────────────────────────────────────

    /// <summary>
    /// Subscribe to the public queue display feed. No authentication required —
    /// intended for wall-mounted TVs at branches.
    /// </summary>
    public async Task JoinQueueDisplay(string branchId)
        => await Groups.AddToGroupAsync(
            Context.ConnectionId,
            QueueDisplayGroup(branchId));
}
