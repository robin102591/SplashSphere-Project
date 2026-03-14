using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;
using SplashSphere.Application.Features.Queue.Commands.CallNextInQueue;

namespace SplashSphere.Application.Features.Queue.Commands.MarkNoShow;

public sealed class MarkNoShowCommandHandler(
    IApplicationDbContext context,
    IEventPublisher eventPublisher,
    ISender sender)
    : IRequestHandler<MarkNoShowCommand, Result>
{
    public async Task<Result> Handle(
        MarkNoShowCommand request,
        CancellationToken cancellationToken)
    {
        var entry = await context.QueueEntries
            .FirstOrDefaultAsync(q => q.Id == request.QueueEntryId, cancellationToken);

        if (entry is null)
            return Result.Failure(Error.NotFound("QueueEntry", request.QueueEntryId));

        // Idempotent guard — if the cashier already started service before the timer
        // fired, there is nothing to do.
        if (entry.Status != QueueStatus.Called)
            return Result.Success();

        var now         = DateTime.UtcNow;
        entry.Status    = QueueStatus.NoShow;
        entry.NoShowAt  = now;

        await eventPublisher.PublishAsync(new QueueEntryNoShowEvent(
            entry.Id,
            entry.TenantId,
            entry.BranchId,
            entry.QueueNumber,
            entry.PlateNumber,
            now), cancellationToken);

        // Auto-call the next WAITING entry in this branch.
        // UnitOfWorkBehavior will SaveChanges for the outer command; this inner
        // Send() call goes through the same pipeline and its SaveChanges is a no-op
        // (UnitOfWorkBehavior only saves at the outermost command level).
        await sender.Send(new CallNextInQueueCommand(entry.BranchId), cancellationToken);

        return Result.Success();
    }
}
