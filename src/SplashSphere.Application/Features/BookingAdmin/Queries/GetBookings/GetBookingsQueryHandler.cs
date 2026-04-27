using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.BookingAdmin.Queries.GetBookings;

public sealed class GetBookingsQueryHandler(
    IApplicationDbContext db)
    : IRequestHandler<GetBookingsQuery, IReadOnlyList<BookingListItemDto>>
{
    private static readonly TimeZoneInfo Manila =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

    public async Task<IReadOnlyList<BookingListItemDto>> Handle(
        GetBookingsQuery request,
        CancellationToken cancellationToken)
    {
        // Inputs are yyyy-mm-dd dates the admin typed in their browser — treat them
        // as Manila-local calendar days and convert to a half-open UTC range
        // [start-of-fromDate, start-of-(toDate+1)). Without this, the upper bound
        // clips bookings on the final day after 08:00 Manila and the lower bound
        // clips the first few hours of the start day.
        var fromLocal = DateTime.SpecifyKind(request.FromDate.Date, DateTimeKind.Unspecified);
        var toLocalExclusive = DateTime.SpecifyKind(
            request.ToDate.Date.AddDays(1), DateTimeKind.Unspecified);

        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(fromLocal, Manila);
        var toUtcExclusive = TimeZoneInfo.ConvertTimeToUtc(toLocalExclusive, Manila);

        // Tenant-scoped — query filters apply automatically on Bookings.
        var query =
            from b in db.Bookings
            join br in db.Branches.IgnoreQueryFilters() on b.BranchId equals br.Id
            join cust in db.Customers.IgnoreQueryFilters() on b.CustomerId equals cust.Id
            join v in db.ConnectVehicles.IgnoreQueryFilters() on b.ConnectVehicleId equals v.Id
            where b.SlotStart >= fromUtc
               && b.SlotStart < toUtcExclusive
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
