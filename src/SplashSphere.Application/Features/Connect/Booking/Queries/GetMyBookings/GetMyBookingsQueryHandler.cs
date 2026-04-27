using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Connect.Booking.Queries.GetMyBookings;

public sealed class GetMyBookingsQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<GetMyBookingsQuery, IReadOnlyList<BookingListItemDto>>
{
    private static readonly BookingStatus[] UpcomingStatuses =
    [
        BookingStatus.Confirmed,
        BookingStatus.Arrived,
        BookingStatus.InService,
    ];

    public async Task<IReadOnlyList<BookingListItemDto>> Handle(
        GetMyBookingsQuery request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated) return [];

        var userId = connectUser.ConnectUserId;
        var nowUtc = DateTime.UtcNow;

        var query =
            from b in db.Bookings.IgnoreQueryFilters()
            join t in db.Tenants.IgnoreQueryFilters() on b.TenantId equals t.Id
            join br in db.Branches.IgnoreQueryFilters() on b.BranchId equals br.Id
            join v in db.ConnectVehicles.IgnoreQueryFilters() on b.ConnectVehicleId equals v.Id
            where b.ConnectUserId == userId
            select new { b, TenantName = t.Name, BranchName = br.Name, Plate = v.PlateNumber };

        if (!request.IncludePast)
        {
            query = query.Where(x => x.b.SlotStart >= nowUtc && UpcomingStatuses.Contains(x.b.Status));
        }

        return await query
            .OrderByDescending(x => x.b.SlotStart)
            .Select(x => new BookingListItemDto(
                x.b.Id,
                x.b.TenantId,
                x.TenantName,
                x.b.BranchId,
                x.BranchName,
                x.b.SlotStart,
                x.b.SlotEnd,
                x.b.Status.ToString(),
                x.b.IsVehicleClassified,
                x.b.EstimatedTotal,
                x.b.EstimatedTotalMin,
                x.b.EstimatedTotalMax,
                x.b.ConnectVehicleId,
                x.Plate))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
