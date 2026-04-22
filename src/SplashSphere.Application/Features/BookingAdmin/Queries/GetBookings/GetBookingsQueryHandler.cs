using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.BookingAdmin.Queries.GetBookings;

public sealed class GetBookingsQueryHandler(
    IApplicationDbContext db)
    : IRequestHandler<GetBookingsQuery, IReadOnlyList<BookingListItemDto>>
{
    public async Task<IReadOnlyList<BookingListItemDto>> Handle(
        GetBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var fromUtc = DateTime.SpecifyKind(request.FromDate, DateTimeKind.Utc);
        var toUtc   = DateTime.SpecifyKind(request.ToDate, DateTimeKind.Utc);

        // Tenant-scoped — query filters apply automatically on Bookings.
        var query =
            from b in db.Bookings
            join br in db.Branches.IgnoreQueryFilters() on b.BranchId equals br.Id
            join cust in db.Customers.IgnoreQueryFilters() on b.CustomerId equals cust.Id
            join v in db.ConnectVehicles.IgnoreQueryFilters() on b.ConnectVehicleId equals v.Id
            where b.SlotStart >= fromUtc
               && b.SlotStart <= toUtc
            select new
            {
                b.Id,
                b.BranchId,
                BranchName = br.Name,
                b.CustomerId,
                CustomerName = cust.FirstName + " " + cust.LastName,
                b.ConnectVehicleId,
                PlateNumber = v.PlateNumber,
                b.SlotStart,
                b.SlotEnd,
                b.Status,
                b.IsVehicleClassified,
                b.EstimatedTotal,
                b.EstimatedTotalMin,
                b.EstimatedTotalMax,
                b.QueueEntryId,
                b.TransactionId,
            };

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(x => x.BranchId == request.BranchId);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var rows = await query
            .AsNoTracking()
            .OrderBy(x => x.SlotStart)
            .ToListAsync(cancellationToken);

        if (rows.Count == 0) return [];

        // Join service names in a second hop (EF composition keeps SQL simple here).
        var bookingIds = rows.Select(r => r.Id).ToList();
        var serviceRows = await (
            from bs in db.BookingServices.IgnoreQueryFilters()
            join s in db.Services.IgnoreQueryFilters() on bs.ServiceId equals s.Id
            where bookingIds.Contains(bs.BookingId)
            select new { bs.BookingId, s.Name })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var servicesByBooking = serviceRows
            .GroupBy(x => x.BookingId)
            .ToDictionary(
                g => g.Key,
                g => string.Join(", ", g.Select(x => x.Name)));

        return rows.Select(r => new BookingListItemDto(
            r.Id,
            r.BranchId,
            r.BranchName,
            r.CustomerId,
            r.CustomerName,
            r.ConnectVehicleId,
            r.PlateNumber,
            servicesByBooking.TryGetValue(r.Id, out var summary) ? summary : string.Empty,
            r.SlotStart,
            r.SlotEnd,
            r.Status.ToString(),
            r.IsVehicleClassified,
            r.EstimatedTotal,
            r.EstimatedTotalMin,
            r.EstimatedTotalMax,
            r.QueueEntryId,
            r.TransactionId)).ToList();
    }
}
