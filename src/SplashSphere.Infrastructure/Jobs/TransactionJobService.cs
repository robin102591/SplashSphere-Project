using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;

namespace SplashSphere.Infrastructure.Jobs;

/// <summary>
/// Hourly Hangfire job that cancels <see cref="TransactionStatus.Pending"/> transactions
/// that have been open for more than 4 hours without progressing to
/// <see cref="TransactionStatus.InProgress"/>.
/// <para>
/// Also cancels any linked <see cref="Domain.Entities.QueueEntry"/> that is still in
/// <see cref="QueueStatus.InService"/> so the queue board is not left in a stale state.
/// </para>
/// Publishes <see cref="TransactionStatusChangedEvent"/> for each cancelled transaction
/// so connected clients are notified via SignalR.
/// </summary>
public sealed class TransactionJobService(
    IServiceScopeFactory scopeFactory,
    ILogger<TransactionJobService> logger)
{
    /// <summary>Transactions sitting in Pending for longer than this are considered stale.</summary>
    private static readonly TimeSpan StaleThreshold = TimeSpan.FromHours(4);

    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 120)]
    public async Task CancelStalePendingTransactionsAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow - StaleThreshold;

        logger.LogInformation(
            "TransactionJob: Scanning for Pending transactions older than {Cutoff:u}.", cutoff);

        using var scope = scopeFactory.CreateScope();
        var db        = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        // Cross-tenant scan.
        var stale = await db.Transactions
            .IgnoreQueryFilters()
            .Where(t =>
                t.Status == TransactionStatus.Pending &&
                t.CreatedAt < cutoff)
            .ToListAsync(ct);

        if (stale.Count == 0)
        {
            logger.LogInformation("TransactionJob: No stale Pending transactions found.");
            return;
        }

        logger.LogInformation(
            "TransactionJob: Cancelling {Count} stale Pending transaction(s).", stale.Count);

        var now = DateTime.UtcNow;

        // Load linked queue entries for the stale transactions in one query.
        var staleIds = stale.Select(t => t.Id).ToList();
        var linkedQueueEntries = await db.QueueEntries
            .IgnoreQueryFilters()
            .Where(q =>
                q.TransactionId != null &&
                staleIds.Contains(q.TransactionId) &&
                q.Status == QueueStatus.InService)
            .ToListAsync(ct);

        var queueMap = linkedQueueEntries
            .Where(q => q.TransactionId != null)
            .ToDictionary(q => q.TransactionId!);

        foreach (var tx in stale)
        {
            tx.Status      = TransactionStatus.Cancelled;
            tx.CancelledAt = now;

            // Cancel the linked queue entry if it is stuck in InService.
            if (queueMap.TryGetValue(tx.Id, out var queueEntry))
            {
                queueEntry.Status      = QueueStatus.Cancelled;
                queueEntry.CancelledAt = now;

                logger.LogInformation(
                    "TransactionJob: Also cancelling linked queue entry {QueueEntryId} ({QueueNumber}).",
                    queueEntry.Id, queueEntry.QueueNumber);
            }

            await publisher.PublishAsync(new TransactionStatusChangedEvent(
                tx.Id,
                tx.TenantId,
                tx.BranchId,
                TransactionStatus.Pending,
                TransactionStatus.Cancelled), ct);

            logger.LogInformation(
                "TransactionJob: Cancelled stale transaction {TxId} ({TxNumber}) for tenant {TenantId}.",
                tx.Id, tx.TransactionNumber, tx.TenantId);
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "TransactionJob: Stale cleanup complete. Cancelled {Count} transaction(s).", stale.Count);
    }
}
