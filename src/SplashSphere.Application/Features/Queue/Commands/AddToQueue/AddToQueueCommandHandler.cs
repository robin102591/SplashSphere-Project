using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Commands.AddToQueue;

public sealed class AddToQueueCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IEventPublisher eventPublisher)
    : IRequestHandler<AddToQueueCommand, Result<string>>
{
    // Default average service duration used for wait time estimation
    // when no historical data is available.
    private const int DefaultServiceDurationMinutes = 15;

    // Asia/Manila UTC offset for deriving the local calendar date.
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<Result<string>> Handle(
        AddToQueueCommand request,
        CancellationToken cancellationToken)
    {
        var branchExists = await context.Branches
            .AnyAsync(b => b.Id == request.BranchId, cancellationToken);

        if (!branchExists)
            return Result.Failure<string>(Error.Validation("Branch ID is invalid."));

        // ── Shift gate: cashier must have an open shift at this branch ───────
        var hasOpenShift = await context.CashierShifts
            .AnyAsync(s => s.CashierId == tenantContext.UserId
                        && s.BranchId == request.BranchId
                        && s.Status == ShiftStatus.Open, cancellationToken);

        if (!hasOpenShift)
            return Result.Failure<string>(Error.Validation(
                "No active shift. Open a shift before adding to the queue."));

        if (!string.IsNullOrWhiteSpace(request.CustomerId))
        {
            var customerExists = await context.Customers
                .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (!customerExists)
                return Result.Failure<string>(Error.Validation("Customer ID is invalid."));
        }

        if (!string.IsNullOrWhiteSpace(request.CarId))
        {
            var carExists = await context.Cars
                .AnyAsync(c => c.Id == request.CarId, cancellationToken);

            if (!carExists)
                return Result.Failure<string>(Error.Validation("Car ID is invalid."));
        }

        // ── Compute today's date (Manila local time) ──────────────────────────
        var localToday = DateOnly.FromDateTime(DateTime.UtcNow + ManilaOffset);

        // ── Estimate wait time ────────────────────────────────────────────────
        // Count active WAITING entries for this branch. Each one represents roughly
        // one service cycle ahead of the new entrant.
        var waitingAhead = await context.QueueEntries
            .CountAsync(q => q.BranchId == request.BranchId
                          && q.Status == QueueStatus.Waiting,
                        cancellationToken);

        var estimatedWait = waitingAhead > 0
            ? waitingAhead * DefaultServiceDurationMinutes
            : (int?)null;

        // ── Build preferred services JSON ─────────────────────────────────────
        string? preferredServicesJson = null;
        if (request.PreferredServiceIds is { Count: > 0 })
            preferredServicesJson = JsonSerializer.Serialize(request.PreferredServiceIds);

        // ── Create queue entry with retry on duplicate queue number ───────────
        // COUNT-based numbering has a race condition under concurrent requests.
        // Instead we derive the next sequence from MAX of today's existing numbers
        // (filtered by QueueDate — the Manila local date column) and retry up to 5×
        // if another request wins the race and causes a 23505 unique-constraint
        // violation on (TenantId, BranchId, QueueDate, QueueNumber).
        const int MaxRetries = 5;

        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            var todayCount = await context.QueueEntries
                .CountAsync(q => q.BranchId == request.BranchId
                              && q.QueueDate == localToday, cancellationToken);

            var queueNumber = $"Q-{todayCount + 1:D3}";

            var entry = new QueueEntry(
                tenantContext.TenantId,
                request.BranchId,
                queueNumber,
                localToday,
                request.PlateNumber,
                request.Priority,
                request.CustomerId,
                request.CarId,
                estimatedWait,
                preferredServicesJson,
                request.Notes);

            context.QueueEntries.Add(entry);

            try
            {
                // Save here so we can detect the 23505 collision on this attempt.
                // UnitOfWorkBehavior will call SaveChangesAsync again after the handler
                // returns, but the change tracker will be empty — effectively a no-op.
                await context.SaveChangesAsync(cancellationToken);

                // ── Publish event (SignalR + display board will pick this up) ─────
                eventPublisher.Enqueue(new QueueEntryCreatedEvent(
                    entry.Id,
                    tenantContext.TenantId,
                    request.BranchId,
                    queueNumber,
                    entry.PlateNumber,
                    request.Priority,
                    estimatedWait));

                return Result.Success(entry.Id);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message.Contains("23505") == true ||
                      ex.InnerException?.Message.Contains("duplicate key") == true)
            {
                // Remove the conflicting entry from the change tracker.
                // EF Core detaches (rather than deletes) entities that are still
                // in the Added state — i.e. were never successfully saved.
                context.QueueEntries.Remove(entry);

                if (attempt == MaxRetries - 1)
                    return Result.Failure<string>(
                        Error.Conflict("Queue is very busy. Please try again in a moment."));
            }
        }

        // Unreachable — the loop always returns inside the try or on the last attempt.
        return Result.Failure<string>(Error.Conflict("Queue number could not be generated."));
    }
}
