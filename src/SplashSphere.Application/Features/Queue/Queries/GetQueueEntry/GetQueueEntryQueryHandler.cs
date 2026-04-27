using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Queries.GetQueueEntry;

public sealed class GetQueueEntryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetQueueEntryQuery, Result<QueueEntryDto>>
{
    public async Task<Result<QueueEntryDto>> Handle(
        GetQueueEntryQuery request,
        CancellationToken cancellationToken)
    {
        var dto = await context.QueueEntries
            .AsNoTracking()
            .Where(q => q.Id == request.QueueEntryId)
            .GroupJoin(
                context.Bookings.AsNoTracking(),
                q => q.Id,
                b => b.QueueEntryId,
                (q, bookings) => new { q, bookings })
            .SelectMany(
                x => x.bookings.DefaultIfEmpty(),
                (x, b) => new QueueEntryDto(
                    x.q.Id,
                    x.q.BranchId,
                    x.q.Branch.Name,
                    x.q.QueueNumber,
                    x.q.PlateNumber,
                    x.q.Status,
                    x.q.Priority,
                    x.q.CustomerId,
                    x.q.Customer != null ? x.q.Customer.FirstName + " " + x.q.Customer.LastName : null,
                    x.q.CarId,
                    x.q.TransactionId,
                    x.q.EstimatedWaitMinutes,
                    x.q.PreferredServices,
                    x.q.Notes,
                    x.q.CalledAt,
                    x.q.StartedAt,
                    x.q.CompletedAt,
                    x.q.CancelledAt,
                    x.q.NoShowAt,
                    x.q.CreatedAt,
                    b != null ? b.Id : null,
                    b != null ? (DateTime?)b.SlotStart : null,
                    b != null ? (bool?)b.IsVehicleClassified : null,
                    b != null ? (BookingStatus?)b.Status : null))
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result.Failure<QueueEntryDto>(Error.NotFound("QueueEntry", request.QueueEntryId))
            : Result.Success(dto);
    }
}
