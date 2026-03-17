using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Builds the full <see cref="QueueDisplayUpdatedPayload"/> snapshot by querying
/// the current queue state for a branch. Used by SignalR notification handlers
/// so the wall-TV display always receives a complete, consistent state.
/// </summary>
internal static class QueueDisplaySnapshotBuilder
{
    public static async Task<QueueDisplayUpdatedPayload> BuildAsync(
        IApplicationDbContext db,
        string branchId,
        CancellationToken ct)
    {
        var activeStatuses = new[] { QueueStatus.Called, QueueStatus.InService, QueueStatus.Waiting };

        var entries = await db.QueueEntries
            .AsNoTracking()
            .IgnoreQueryFilters()   // public display — no tenant filter
            .Where(q => q.BranchId == branchId && activeStatuses.Contains(q.Status))
            .OrderByDescending(q => q.Priority)
            .ThenBy(q => q.CreatedAt)
            .Select(q => new { q.QueueNumber, q.PlateNumber, q.Status, q.Priority, q.EstimatedWaitMinutes })
            .ToListAsync(ct);

        var calling = entries
            .Where(e => e.Status == QueueStatus.Called)
            .Select(e => new QueueDisplayEntryPayload(
                e.QueueNumber,
                MaskPlate(e.PlateNumber),
                (int)e.Status,
                (int)e.Priority,
                e.EstimatedWaitMinutes))
            .ToList();

        var inService = entries
            .Where(e => e.Status == QueueStatus.InService)
            .Select(e => new QueueDisplayEntryPayload(
                e.QueueNumber,
                MaskPlate(e.PlateNumber),
                (int)e.Status,
                (int)e.Priority,
                e.EstimatedWaitMinutes))
            .ToList();

        var waitingCount = entries.Count(e => e.Status == QueueStatus.Waiting);

        return new QueueDisplayUpdatedPayload(branchId, calling, inService, waitingCount);
    }

    private static string MaskPlate(string plate)
    {
        if (string.IsNullOrWhiteSpace(plate) || plate.Length < 5)
            return "***";
        return $"{plate[..3]}***{plate[^2..]}";
    }
}
