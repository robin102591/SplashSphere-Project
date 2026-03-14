using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Commands.RequeueEntry;

public sealed class RequeueEntryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RequeueEntryCommand, Result>
{
    private const int DefaultServiceDurationMinutes = 15;

    public async Task<Result> Handle(
        RequeueEntryCommand request,
        CancellationToken cancellationToken)
    {
        var entry = await context.QueueEntries
            .FirstOrDefaultAsync(q => q.Id == request.QueueEntryId, cancellationToken);

        if (entry is null)
            return Result.Failure(Error.NotFound("QueueEntry", request.QueueEntryId));

        if (entry.Status != QueueStatus.NoShow)
            return Result.Failure(Error.Validation(
                $"Only NoShow entries can be re-queued. Current status: {entry.Status}."));

        // Recalculate estimated wait.
        var waitingAhead = await context.QueueEntries
            .CountAsync(q => q.BranchId == entry.BranchId && q.Status == QueueStatus.Waiting,
                        cancellationToken);

        entry.Status               = QueueStatus.Waiting;
        entry.NoShowAt             = null;
        entry.CalledAt             = null;
        entry.EstimatedWaitMinutes = waitingAhead > 0 ? waitingAhead * DefaultServiceDurationMinutes : null;

        if (request.NewPriority.HasValue)
            entry.Priority = request.NewPriority.Value;

        return Result.Success();
    }
}
