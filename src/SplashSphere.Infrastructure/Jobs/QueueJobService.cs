using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Queue.Commands.MarkNoShow;
using SplashSphere.Infrastructure.Auth;

namespace SplashSphere.Infrastructure.Jobs;

/// <summary>
/// Safety-net Hangfire job (every 5 minutes) that finds queue entries stuck in
/// <see cref="Domain.Enums.QueueStatus.Called"/> state for more than 5 minutes
/// and marks them as NoShow.
/// <para>
/// This is a backup sweep for cases where the per-entry Hangfire delayed job
/// (scheduled by <see cref="Services.BackgroundJobService.ScheduleNoShowCheck"/>)
/// did not fire — e.g. a server restart between the delay being scheduled and
/// the job executing.
/// </para>
/// <para>
/// Dispatches <see cref="MarkNoShowCommand"/> per entry (within a per-tenant scope)
/// so all side effects are identical to the normal no-show path:
/// entry → NoShow, publish event, auto-call next WAITING entry.
/// </para>
/// <b>Idempotent:</b> <see cref="MarkNoShowCommandHandler"/> checks the status before
/// acting, so dispatching for an already-resolved entry is safe.
/// </summary>
public sealed class QueueJobService(
    IServiceScopeFactory scopeFactory,
    ILogger<QueueJobService> logger)
{
    /// <summary>
    /// Entries in Called state longer than this without service starting are NoShow candidates.
    /// Must match the delay used in <c>BackgroundJobService.ScheduleNoShowCheck</c>.
    /// </summary>
    private static readonly TimeSpan NoShowTimeout = TimeSpan.FromMinutes(5);

    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task MarkStuckNoShowsAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow - NoShowTimeout;

        logger.LogDebug(
            "QueueJob: Scanning for Called entries with CalledAt < {Cutoff:u}.", cutoff);

        // ── Cross-tenant scan ─────────────────────────────────────────────────
        List<(string Id, string TenantId)> stuck;
        using (var scanScope = scopeFactory.CreateScope())
        {
            var db = scanScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            stuck = await db.QueueEntries
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(q =>
                    q.Status == Domain.Enums.QueueStatus.Called &&
                    q.CalledAt != null &&
                    q.CalledAt < cutoff)
                .Select(q => new { q.Id, q.TenantId })
                .ToListAsync(ct)
                .ContinueWith(t => t.Result.Select(x => (x.Id, x.TenantId)).ToList(), ct);
        }

        if (stuck.Count == 0)
        {
            logger.LogDebug("QueueJob: No stuck Called entries found.");
            return;
        }

        logger.LogInformation(
            "QueueJob: Found {Count} stuck Called entry(ies). Marking as NoShow.", stuck.Count);

        var marked = 0;
        var skipped = 0;

        foreach (var (entryId, tenantId) in stuck)
        {
            // Per-tenant scope ensures the global query filter and event publisher
            // work correctly with the right tenant context.
            using var tenantScope = scopeFactory.CreateScope();

            var tenantCtx = tenantScope.ServiceProvider.GetRequiredService<TenantContext>();
            tenantCtx.TenantId = tenantId;

            var mediator = tenantScope.ServiceProvider.GetRequiredService<MediatR.ISender>();

            try
            {
                var result = await mediator.Send(new MarkNoShowCommand(entryId), ct);

                if (result.IsSuccess)
                {
                    marked++;
                    logger.LogInformation(
                        "QueueJob: Marked entry {EntryId} as NoShow (tenant {TenantId}).",
                        entryId, tenantId);
                }
                else
                {
                    // IsSuccess with no-op means status was already resolved (idempotent guard).
                    skipped++;
                    logger.LogDebug(
                        "QueueJob: Entry {EntryId} already resolved — skipped ({Error}).",
                        entryId, result.Error);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "QueueJob: Exception marking entry {EntryId} as NoShow.", entryId);
            }
        }

        logger.LogInformation(
            "QueueJob: NoShow sweep complete. Marked={Marked}, Skipped={Skipped}.",
            marked, skipped);
    }
}
