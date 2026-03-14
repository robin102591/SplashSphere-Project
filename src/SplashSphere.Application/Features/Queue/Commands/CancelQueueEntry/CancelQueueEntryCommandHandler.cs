using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Commands.CancelQueueEntry;

public sealed class CancelQueueEntryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelQueueEntryCommand, Result>
{
    public async Task<Result> Handle(
        CancelQueueEntryCommand request,
        CancellationToken cancellationToken)
    {
        var entry = await context.QueueEntries
            .FirstOrDefaultAsync(q => q.Id == request.QueueEntryId, cancellationToken);

        if (entry is null)
            return Result.Failure(Error.NotFound("QueueEntry", request.QueueEntryId));

        if (entry.Status is not (QueueStatus.Waiting or QueueStatus.Called))
            return Result.Failure(Error.Validation(
                $"Queue entry cannot be cancelled from {entry.Status} status. " +
                $"Only Waiting and Called entries can be cancelled."));

        entry.Status      = QueueStatus.Cancelled;
        entry.CancelledAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Reason))
            entry.Notes = string.IsNullOrWhiteSpace(entry.Notes)
                ? request.Reason
                : $"{entry.Notes} | Cancelled: {request.Reason}";

        return Result.Success();
    }
}
