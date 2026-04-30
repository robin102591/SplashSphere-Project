namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Pushes transaction-lifecycle events to the customer-facing display paired
/// to the transaction's POS station. Implementation lives in Infrastructure
/// (it builds the customer-safe DTO and dispatches via SignalR).
/// <para>
/// All methods are no-ops when the transaction has no <c>PosStationId</c> —
/// transactions created from the admin app or from older sessions simply
/// don't drive a display. Failures are swallowed so a SignalR hiccup never
/// tears down the rest of the transaction-completion pipeline (loyalty, SMS,
/// payroll commission accumulation, etc.).
/// </para>
/// </summary>
public interface IDisplayBroadcaster
{
    /// <summary>Fired once when the transaction is first created. Display: Idle → Building.</summary>
    Task BroadcastStartedAsync(string transactionId, CancellationToken cancellationToken);

    /// <summary>Fired on every line-item / discount / customer-link change while in Building.</summary>
    Task BroadcastUpdatedAsync(string transactionId, CancellationToken cancellationToken);

    /// <summary>Fired when the final payment lands. Display: Building → Complete (auto-reverts to Idle).</summary>
    Task BroadcastCompletedAsync(string transactionId, CancellationToken cancellationToken);

    /// <summary>Fired on void/cancel from any non-terminal state. Display: any → Idle.</summary>
    Task BroadcastCancelledAsync(string transactionId, CancellationToken cancellationToken);

    /// <summary>
    /// Force-reverts a specific station's display to Idle without touching any
    /// transaction. Used when the cashier parks a transaction (Pay Later) and
    /// walks away — from the customer's perspective the screen should be ready
    /// for the next person, even though the transaction is still Pending in
    /// the DB. Unlike <see cref="BroadcastCancelledAsync"/> which routes by
    /// transaction → station, this targets the station directly.
    /// </summary>
    Task ClearStationAsync(string branchId, string stationId, CancellationToken cancellationToken);
}
