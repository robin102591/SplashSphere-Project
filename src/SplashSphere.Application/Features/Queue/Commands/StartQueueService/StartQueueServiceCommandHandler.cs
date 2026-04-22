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

        // If this queue entry came from an online booking, enforce the
        // classification guard and mirror the transition onto the booking.
        var linkedBooking = await context.Bookings
            .FirstOrDefaultAsync(b => b.QueueEntryId == entry.Id, cancellationToken);

        if (linkedBooking is not null && !linkedBooking.IsVehicleClassified)
            return Result.Failure(Error.Validation(
                "BOOKING_VEHICLE_NOT_CLASSIFIED",
                "Classify the vehicle before starting service."));

        var now = DateTime.UtcNow;
        entry.Status        = QueueStatus.InService;
        entry.TransactionId = request.TransactionId;
        entry.StartedAt     = now;

        if (linkedBooking is not null)
        {
            linkedBooking.TransactionId = request.TransactionId;
            linkedBooking.Status        = BookingStatus.InService;
        }

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
