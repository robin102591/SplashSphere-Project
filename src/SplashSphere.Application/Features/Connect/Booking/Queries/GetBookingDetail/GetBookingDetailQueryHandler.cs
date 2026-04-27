using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Booking.Queries.GetBookingDetail;

public sealed class GetBookingDetailQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<GetBookingDetailQuery, BookingDetailDto?>
{
    public async Task<BookingDetailDto?> Handle(
        GetBookingDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated) return null;
        var userId = connectUser.ConnectUserId;

        var row = await (
            from b in db.Bookings.IgnoreQueryFilters()
            join t in db.Tenants.IgnoreQueryFilters() on b.TenantId equals t.Id
            join br in db.Branches.IgnoreQueryFilters() on b.BranchId equals br.Id
            join v in db.ConnectVehicles.IgnoreQueryFilters() on b.ConnectVehicleId equals v.Id
            join q in db.QueueEntries.IgnoreQueryFilters()
                on b.QueueEntryId equals q.Id into queueJoin
            from q in queueJoin.DefaultIfEmpty()
            where b.Id == request.BookingId && b.ConnectUserId == userId
            select new
            {
                b,
                TenantName = t.Name,
                BranchName = br.Name,
                Plate = v.PlateNumber,
                QueueNumber = q == null ? null : q.QueueNumber,
                QueueStatus = q == null ? (string?)null : q.Status.ToString(),
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null) return null;

        var services = await (
            from bs in db.BookingServices.IgnoreQueryFilters()
            join s in db.Services.IgnoreQueryFilters() on bs.ServiceId equals s.Id
            where bs.BookingId == row.b.Id
            orderby s.Name
            select new BookingServiceDto(s.Id, s.Name, bs.Price, bs.PriceMin, bs.PriceMax))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new BookingDetailDto(
            row.b.Id,
            row.b.TenantId,
            row.TenantName,
            row.b.BranchId,
            row.BranchName,
            row.b.SlotStart,
            row.b.SlotEnd,
            row.b.Status.ToString(),
            row.b.IsVehicleClassified,
            row.b.EstimatedTotal,
            row.b.EstimatedTotalMin,
            row.b.EstimatedTotalMax,
            row.b.EstimatedDurationMinutes,
            row.b.ConnectVehicleId,
            row.Plate,
            row.b.QueueEntryId,
            row.QueueNumber,
            row.QueueStatus,
            row.b.TransactionId,
            services);
    }
}
