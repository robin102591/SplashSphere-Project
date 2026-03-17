using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Commands.StartQueueService;

public sealed class StartQueueServiceCommandHandler(
    IApplicationDbContext context,
    IEventPublisher eventPublisher)
    : IRequestHandler<StartQueueServiceCommand, Result>
{
    public async Task<Result> Handle(
        StartQueueServiceCommand request,
        CancellationToken cancellationToken)
    {
        var entry = await context.QueueEntries
            .FirstOrDefaultAsync(q => q.Id == request.QueueEntryId, cancellationToken);

        if (entry is null)
            return Result.Failure(Error.NotFound("QueueEntry", request.QueueEntryId));

        if (entry.Status != QueueStatus.Called)
            return Result.Failure(Error.Validation(
                $"Queue entry must be in Called status to start service. Current status: {entry.Status}."));

        var transactionExists = await context.Transactions
            .AnyAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (!transactionExists)
            return Result.Failure(Error.NotFound("Transaction", request.TransactionId));

        var now = DateTime.UtcNow;
        entry.Status        = QueueStatus.InService;
        entry.TransactionId = request.TransactionId;
        entry.StartedAt     = now;

        eventPublisher.Enqueue(new QueueEntryInServiceEvent(
            entry.Id,
            entry.TenantId,
            entry.BranchId,
            entry.QueueNumber,
            entry.PlateNumber,
            now));

        return Result.Success();
    }
}
