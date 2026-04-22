using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Auth;

namespace SplashSphere.Infrastructure.Jobs;

/// <summary>
/// Hangfire jobs that bridge online <see cref="Booking"/>s into the physical queue
/// and clean up no-shows.
/// <para>
/// <b>CreateQueueFromBookings</b> — every 5 minutes, find Confirmed/Arrived bookings
/// whose slot is imminent (≤ 15 min away) and have no <see cref="QueueEntry"/> yet.
/// Creates a queue entry at <see cref="QueuePriority.Booked"/> priority so the vehicle
/// is auto-seated above walk-ins.
/// </para>
/// <para>
/// <b>MarkBookingNoShows</b> — every 5 minutes, find still-Confirmed bookings whose
/// slot ended more than <c>NoShowGraceMinutes</c> ago (branch setting) and mark them
/// as <see cref="BookingStatus.NoShow"/>. Does nothing if the customer already checked
/// in (Arrived/InService/Completed).
/// </para>
/// </summary>
public sealed class BookingJobService(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingJobService> logger)
{
    /// <summary>
    /// Look-ahead window — bookings starting within this window are auto-enqueued.
    /// Matches the "~15 minutes before slot" contract in <see cref="Booking"/>'s doc.
    /// </summary>
    private static readonly TimeSpan LookAheadWindow = TimeSpan.FromMinutes(15);

    private const int DefaultServiceDurationMinutes = 15;
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task CreateQueueFromBookingsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var cutoff = now + LookAheadWindow;

        logger.LogDebug(
            "BookingJob: Scanning for bookings with SlotStart <= {Cutoff:u} and no queue entry.",
            cutoff);

        // ── Cross-tenant scan ────────────────────────────────────────────────
        List<(string BookingId, string TenantId, string BranchId, string PlateNumber,
              string? CustomerId, string? CarId, int EstimatedDurationMinutes)> candidates;

        using (var scanScope = scopeFactory.CreateScope())
        {
            var db = scanScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            candidates = await (
                from b in db.Bookings.IgnoreQueryFilters()
                join v in db.ConnectVehicles.IgnoreQueryFilters() on b.ConnectVehicleId equals v.Id
                where b.QueueEntryId == null
                   && b.SlotStart <= cutoff
                   && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Arrived)
                select new
                {
                    BookingId = b.Id,
                    b.TenantId,
                    b.BranchId,
                    PlateNumber = v.PlateNumber,
                    b.CustomerId,
                    b.CarId,
                    b.EstimatedDurationMinutes,
                })
                .AsNoTracking()
                .ToListAsync(ct)
                .ContinueWith(t => t.Result
                    .Select(x => (x.BookingId, x.TenantId, x.BranchId, x.PlateNumber,
                                  (string?)x.CustomerId, (string?)x.CarId, x.EstimatedDurationMinutes))
                    .ToList(), ct);
        }

        if (candidates.Count == 0)
        {
            logger.LogDebug("BookingJob: No bookings eligible for auto-enqueue.");
            return;
        }

        logger.LogInformation(
            "BookingJob: Found {Count} booking(s) to enqueue.", candidates.Count);

        var created = 0;
        var skipped = 0;

        foreach (var c in candidates)
        {
            using var tenantScope = scopeFactory.CreateScope();

            var tenantCtx = tenantScope.ServiceProvider.GetRequiredService<TenantContext>();
            tenantCtx.TenantId = c.TenantId;

            var db = tenantScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var eventPublisher = tenantScope.ServiceProvider.GetRequiredService<IEventPublisher>();

            try
            {
                // Re-load inside the tenant scope so query filters apply + we can
                // track the booking for update. Re-check the guards — another run
                // or the customer-side Arrived flow may have set QueueEntryId already.
                var booking = await db.Bookings
                    .FirstOrDefaultAsync(b => b.Id == c.BookingId, ct);

                if (booking is null || booking.QueueEntryId is not null)
                {
                    skipped++;
                    continue;
                }

                if (booking.Status != BookingStatus.Confirmed
                    && booking.Status != BookingStatus.Arrived)
                {
                    skipped++;
                    continue;
                }

                // Waiting ahead (excluding terminals) — rough wait estimate.
                var waitingAhead = await db.QueueEntries
                    .CountAsync(q => q.BranchId == c.BranchId
                                  && q.Status == QueueStatus.Waiting, ct);

                var estimatedWait = waitingAhead > 0
                    ? waitingAhead * DefaultServiceDurationMinutes
                    : (int?)null;

                var localToday = DateOnly.FromDateTime(DateTime.UtcNow + ManilaOffset);

                // ── Create queue entry with retry on duplicate queue number ─
                const int MaxRetries = 5;
                QueueEntry? created_entry = null;

                for (var attempt = 0; attempt < MaxRetries; attempt++)
                {
                    var todayCount = await db.QueueEntries
                        .CountAsync(q => q.BranchId == c.BranchId
                                      && q.QueueDate == localToday, ct);

                    var queueNumber = $"Q-{todayCount + 1:D3}";

                    var entry = new QueueEntry(
                        tenantId: c.TenantId,
                        branchId: c.BranchId,
                        queueNumber: queueNumber,
                        queueDate: localToday,
                        plateNumber: c.PlateNumber,
                        priority: QueuePriority.Booked,
                        customerId: c.CustomerId,
                        carId: c.CarId,
                        estimatedWaitMinutes: estimatedWait,
                        preferredServices: null,
                        notes: $"Booking {c.BookingId[..8]}");

                    db.QueueEntries.Add(entry);
                    booking.QueueEntryId = entry.Id;

                    try
                    {
                        await db.SaveChangesAsync(ct);
                        created_entry = entry;

                        eventPublisher.Enqueue(new QueueEntryCreatedEvent(
                            entry.Id,
                            c.TenantId,
                            c.BranchId,
                            queueNumber,
                            entry.PlateNumber,
                            QueuePriority.Booked,
                            estimatedWait));

                        await eventPublisher.FlushAsync(ct);
                        break;
                    }
                    catch (DbUpdateException ex)
                        when (ex.InnerException?.Message.Contains("23505") == true ||
                              ex.InnerException?.Message.Contains("duplicate key") == true)
                    {
                        db.QueueEntries.Remove(entry);
                        booking.QueueEntryId = null;

                        if (attempt == MaxRetries - 1)
                        {
                            logger.LogWarning(
                                "BookingJob: Gave up generating queue number for booking {BookingId}.",
                                c.BookingId);
                        }
                    }
                }

                if (created_entry is not null)
                {
                    created++;
                    logger.LogInformation(
                        "BookingJob: Enqueued booking {BookingId} as {QueueNumber} (tenant {TenantId}).",
                        c.BookingId, created_entry.QueueNumber, c.TenantId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "BookingJob: Exception enqueuing booking {BookingId}.", c.BookingId);
            }
        }

        logger.LogInformation(
            "BookingJob: Enqueue sweep complete. Created={Created}, Skipped={Skipped}.",
            created, skipped);
    }

    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task MarkBookingNoShowsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        logger.LogDebug(
            "BookingJob: Scanning for overdue Confirmed bookings as of {Now:u}.", now);

        // ── Cross-tenant scan ────────────────────────────────────────────────
        List<(string BookingId, string TenantId, int GraceMinutes, DateTime SlotEnd)> overdue;

        using (var scanScope = scopeFactory.CreateScope())
        {
            var db = scanScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            // Join to BookingSetting for the per-branch grace minutes; fall back
            // to the entity default (15) if no settings row exists.
            overdue = await (
                from b in db.Bookings.IgnoreQueryFilters()
                join s in db.BookingSettings.IgnoreQueryFilters()
                    on new { b.TenantId, b.BranchId } equals new { s.TenantId, s.BranchId }
                    into settingsJoin
                from s in settingsJoin.DefaultIfEmpty()
                where b.Status == BookingStatus.Confirmed
                select new
                {
                    BookingId = b.Id,
                    b.TenantId,
                    b.SlotEnd,
                    GraceMinutes = s != null ? s.NoShowGraceMinutes : 15,
                })
                .AsNoTracking()
                .ToListAsync(ct)
                .ContinueWith(t => t.Result
                    .Where(x => x.SlotEnd.AddMinutes(x.GraceMinutes) < now)
                    .Select(x => (x.BookingId, x.TenantId, x.GraceMinutes, x.SlotEnd))
                    .ToList(), ct);
        }

        if (overdue.Count == 0)
        {
            logger.LogDebug("BookingJob: No overdue Confirmed bookings.");
            return;
        }

        logger.LogInformation(
            "BookingJob: Found {Count} overdue booking(s). Marking as NoShow.", overdue.Count);

        var marked = 0;
        var skipped = 0;

        foreach (var o in overdue)
        {
            using var tenantScope = scopeFactory.CreateScope();

            var tenantCtx = tenantScope.ServiceProvider.GetRequiredService<TenantContext>();
            tenantCtx.TenantId = o.TenantId;

            var db = tenantScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            try
            {
                var booking = await db.Bookings
                    .FirstOrDefaultAsync(b => b.Id == o.BookingId, ct);

                // Re-check: only flip if still Confirmed. Anyone who moved past
                // Confirmed (Arrived/InService/Completed/Cancelled) is safe.
                if (booking is null || booking.Status != BookingStatus.Confirmed)
                {
                    skipped++;
                    continue;
                }

                booking.Status = BookingStatus.NoShow;
                await db.SaveChangesAsync(ct);

                marked++;
                logger.LogInformation(
                    "BookingJob: Booking {BookingId} marked NoShow (tenant {TenantId}, slot ended {SlotEnd:u}).",
                    o.BookingId, o.TenantId, o.SlotEnd);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "BookingJob: Exception marking booking {BookingId} as NoShow.", o.BookingId);
            }
        }

        logger.LogInformation(
            "BookingJob: NoShow sweep complete. Marked={Marked}, Skipped={Skipped}.",
            marked, skipped);
    }
}
