using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Display.DTOs;
using SplashSphere.Infrastructure.Hubs;

namespace SplashSphere.Infrastructure.ExternalServices;

/// <summary>
/// SignalR-backed implementation of <see cref="IDisplayBroadcaster"/>. Builds
/// customer-safe payloads (via <see cref="DisplayTransactionLoader"/>) and
/// dispatches them to the station's display group.
/// <para>
/// All exceptions are caught and logged at warning level — a SignalR hiccup
/// must never tear down the rest of the transaction-completion pipeline.
/// </para>
/// </summary>
public sealed class SignalRDisplayBroadcaster(
    IHubContext<SplashSphereHub> hub,
    IApplicationDbContext db,
    ILogger<SignalRDisplayBroadcaster> logger)
    : IDisplayBroadcaster
{
    public async Task BroadcastStartedAsync(string transactionId, CancellationToken cancellationToken)
        => await SafeDispatchAsync(transactionId, "DisplayTransactionStarted", cancellationToken);

    public async Task BroadcastUpdatedAsync(string transactionId, CancellationToken cancellationToken)
        => await SafeDispatchAsync(transactionId, "DisplayTransactionUpdated", cancellationToken);

    public async Task BroadcastCompletedAsync(string transactionId, CancellationToken cancellationToken)
    {
        try
        {
            var (group, payload) = await DisplayTransactionLoader
                .LoadCompletionAsync(db, transactionId, cancellationToken);

            if (group is null || payload is null) return;

            await hub.Clients
                .Group(group)
                .SendAsync("DisplayTransactionCompleted", payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast DisplayTransactionCompleted for {TransactionId}.",
                transactionId);
        }
    }

    public async Task BroadcastCancelledAsync(string transactionId, CancellationToken cancellationToken)
    {
        try
        {
            var route = await db.Transactions
                .AsNoTracking()
                .Where(t => t.Id == transactionId && t.PosStationId != null)
                .Select(t => new { t.BranchId, t.PosStationId })
                .FirstOrDefaultAsync(cancellationToken);

            if (route is null || route.PosStationId is null) return;

            var group = SplashSphereHub.CustomerDisplayGroup(route.BranchId, route.PosStationId);
            await hub.Clients.Group(group).SendAsync(
                "DisplayTransactionCancelled",
                new { },
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast DisplayTransactionCancelled for {TransactionId}.",
                transactionId);
        }
    }

    public async Task ClearStationAsync(
        string branchId,
        string stationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var group = SplashSphereHub.CustomerDisplayGroup(branchId, stationId);
            await hub.Clients.Group(group).SendAsync(
                "DisplayTransactionCancelled",
                new { },
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to clear display for station {BranchId}/{StationId}.",
                branchId, stationId);
        }
    }

    private async Task SafeDispatchAsync(
        string transactionId,
        string eventName,
        CancellationToken cancellationToken)
    {
        try
        {
            var (group, payload) = await DisplayTransactionLoader
                .LoadTransactionAsync(db, transactionId, cancellationToken);

            if (group is null || payload is null) return;

            await hub.Clients.Group(group).SendAsync(eventName, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast {EventName} for {TransactionId}.",
                eventName, transactionId);
        }
    }
}
