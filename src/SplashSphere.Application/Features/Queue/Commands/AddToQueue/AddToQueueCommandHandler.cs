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

        // ── Generate queue number ─────────────────────────────────────────────
        var localToday = DateOnly.FromDateTime(DateTime.UtcNow + ManilaOffset);
        var todayStart = localToday.ToDateTime(TimeOnly.MinValue);
        var todayEnd   = localToday.ToDateTime(TimeOnly.MaxValue);

        var todayCount = await context.QueueEntries
            .CountAsync(q => q.BranchId == request.BranchId
                          && q.CreatedAt >= todayStart
                          && q.CreatedAt <= todayEnd,
                        cancellationToken);

        var queueNumber = $"Q-{todayCount + 1:D3}";

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

        // ── Create queue entry ────────────────────────────────────────────────
        var entry = new QueueEntry(
            tenantContext.TenantId,
            request.BranchId,
            queueNumber,
            request.PlateNumber,
            request.Priority,
            request.CustomerId,
            request.CarId,
            estimatedWait,
            preferredServicesJson,
            request.Notes);

        context.QueueEntries.Add(entry);

        // ── Publish event (SignalR + display board will pick this up) ─────────
        await eventPublisher.PublishAsync(new QueueEntryCreatedEvent(
            entry.Id,
            tenantContext.TenantId,
            request.BranchId,
            queueNumber,
            entry.PlateNumber,
            request.Priority,
            estimatedWait), cancellationToken);

        return Result.Success(entry.Id);
    }
}
