using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Queries.GetQueue;

public sealed class GetQueueQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetQueueQuery, PagedResult<QueueEntryDto>>
{
    private static readonly QueueStatus[] DefaultStatuses =
        [QueueStatus.Waiting, QueueStatus.Called, QueueStatus.InService];

    public async Task<PagedResult<QueueEntryDto>> Handle(
        GetQueueQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = request.Statuses is { Count: > 0 }
            ? request.Statuses
            : DefaultStatuses;

        var query = context.QueueEntries
            .AsNoTracking()
            .Where(q => q.BranchId == request.BranchId && statuses.Contains(q.Status));

        var totalCount = await query.CountAsync(cancellationToken);

        // LEFT JOIN on Bookings via QueueEntryId — walk-ins have no booking row,
        // so the booking-derived fields come back null for them.
        var items = await query
            .OrderByDescending(q => q.Priority)
            .ThenBy(q => q.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
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
            .ToListAsync(cancellationToken);

        return PagedResult<QueueEntryDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
