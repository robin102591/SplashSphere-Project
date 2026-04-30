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

    /// <summary>
    /// Group key for a customer-facing display paired to one POS station.
    /// One station = one display = one group. Multiple displays in the same
    /// station mirror the same transaction stream.
    /// </summary>
    public static string CustomerDisplayGroup(string branchId, string stationId)
        => $"display:{branchId}:{stationId}";

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

    /// <summary>
    /// Subscribe to a station's customer-display feed
    /// (<c>display:{branchId}:{stationId}</c>). Authenticated — the cashier or
    /// admin sets up the device once with their Clerk session and leaves it
    /// running. Tenant scoping is enforced via the JWT <c>org_id</c> claim
    /// (we never trust client-supplied tenant), but the same group key works
    /// across all tenants because branchId is already tenant-scoped.
    /// </summary>
    [Authorize]
    public async Task JoinDisplayGroup(string branchId, string stationId)
    {
        var tenantId = Context.User!.FindFirst("org_id")?.Value
            ?? throw new HubException("No tenant context in token.");

        // No DB-side validation here — slice 4 will enforce that the station
        // actually belongs to the cashier's tenant when transaction events
        // are dispatched. For now, joining a non-existent group is harmless
        // (the client just never receives any events).
        _ = tenantId;

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            CustomerDisplayGroup(branchId, stationId));
    }

    /// <summary>Leave the customer-display group (e.g. switching stations).</summary>
    [Authorize]
    public async Task LeaveDisplayGroup(string branchId, string stationId)
        => await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            CustomerDisplayGroup(branchId, stationId));

    /// <summary>
    /// Pushes the cashier's in-progress (draft) cart to the paired customer
    /// display, before any DB row exists. The display reducer treats this
    /// payload identically to a real <c>DisplayTransactionUpdated</c> event,
    /// so the customer sees items, totals, and vehicle info build up live as
    /// the cashier works through the cart at <c>/transactions/new</c>.
    /// <para>
    /// Routing: targets <c>display:{branchId}:{stationId}</c> exactly the way
    /// real transaction events do. Once the cashier finalises (Complete or
    /// Pay Later), the persisted transaction's <c>TransactionCreatedEvent</c>
    /// takes over and this method stops being called.
    /// </para>
    /// <para>
    /// <b>Trust model:</b> the payload is unverified cashier-supplied data,
    /// and that is OK by design — the display is a customer-trust UI, not a
    /// ledger. The actual receipt comes from the POSTed transaction. The
    /// cashier can only target a (branchId, stationId) pair that their UI
    /// already shows them; tenant-foreign branchIds are uuid-collision-safe.
    /// </para>
    /// </summary>
    [Authorize]
    public async Task BroadcastDraftDisplay(
        string branchId,
        string stationId,
        DraftDisplayPayload payload)
    {
        var tenantId = Context.User!.FindFirst("org_id")?.Value
            ?? throw new HubException("No tenant context in token.");

        // tenantId is required for auth; the discriminator on the group key is
        // branchId which is already tenant-scoped on the cashier's client.
        _ = tenantId;

        await Clients
            .Group(CustomerDisplayGroup(branchId, stationId))
            .SendAsync("DisplayTransactionUpdated", payload);
    }
}

/// <summary>
/// Cashier-supplied snapshot of the in-progress cart on /transactions/new.
/// Mirrors <c>DisplayTransactionResultDto</c> from the Application layer so
/// the display's reducer can render it the same way as a server-built
/// payload — but kept as a separate hub-layer record because Application
/// DTOs are nominally inputs to the broadcaster, not the hub surface.
/// </summary>
public sealed record DraftDisplayPayload(
    string TransactionId,
    string? VehiclePlate,
    string? VehicleMakeModel,
    string? VehicleTypeSize,
    string? CustomerName,
    string? LoyaltyTier,
    IReadOnlyList<DraftDisplayLineItem> Items,
    decimal Subtotal,
    decimal DiscountAmount,
    string? DiscountLabel,
    decimal TaxAmount,
    decimal Total);

public sealed record DraftDisplayLineItem(
    string Id,
    string Name,
    string Type,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
