using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.BookingAdmin.Queries.GetBookingDetailAdmin;

public sealed class GetBookingDetailAdminQueryHandler(
    IApplicationDbContext db)
    : IRequestHandler<GetBookingDetailAdminQuery, BookingAdminDetailDto?>
{
    public async Task<BookingAdminDetailDto?> Handle(
        GetBookingDetailAdminQuery request,
        CancellationToken cancellationToken)
    {
        var row = await (
            from b in db.Bookings
            join br in db.Branches.IgnoreQueryFilters() on b.BranchId equals br.Id
            join cust in db.Customers.IgnoreQueryFilters() on b.CustomerId equals cust.Id
            join v in db.ConnectVehicles.IgnoreQueryFilters() on b.ConnectVehicleId equals v.Id
            join make in db.GlobalMakes.IgnoreQueryFilters() on v.MakeId equals make.Id into makeJoin
            from make in makeJoin.DefaultIfEmpty()
            join model in db.GlobalModels.IgnoreQueryFilters() on v.ModelId equals model.Id into modelJoin
            from model in modelJoin.DefaultIfEmpty()
            where b.Id == request.BookingId
            select new
            {
                b.Id,
                b.BranchId,
                BranchName = br.Name,
                b.CustomerId,
                CustomerName = cust.FirstName + " " + cust.LastName,
                CustomerPhone = cust.ContactNumber,
                b.ConnectVehicleId,
                PlateNumber = v.PlateNumber,
                MakeName = make != null ? make.Name : null,
                ModelName = model != null ? model.Name : null,
                b.SlotStart,
                b.SlotEnd,
                b.EstimatedDurationMinutes,
                b.Status,
                b.IsVehicleClassified,
                b.EstimatedTotal,
                b.EstimatedTotalMin,
                b.EstimatedTotalMax,
                b.CancellationReason,
                b.QueueEntryId,
                b.TransactionId,
                b.CreatedAt,
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null) return null;

        var services = await (
            from bs in db.BookingServices
            join s in db.Services.IgnoreQueryFilters() on bs.ServiceId equals s.Id
            where bs.BookingId == request.BookingId
            select new BookingAdminServiceDto(
                s.Id,
                s.Name,
                bs.Price,
                bs.PriceMin,
                bs.PriceMax))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new BookingAdminDetailDto(
            row.Id,
            row.BranchId,
            row.BranchName,
            row.CustomerId,
            row.CustomerName,
            row.CustomerPhone,
            row.ConnectVehicleId,
            row.PlateNumber,
            row.MakeName,
            row.ModelName,
            row.SlotStart,
            row.SlotEnd,
            row.EstimatedDurationMinutes,
            row.Status.ToString(),
            row.IsVehicleClassified,
            row.EstimatedTotal,
            row.EstimatedTotalMin,
            row.EstimatedTotalMax,
            row.CancellationReason,
            row.QueueEntryId,
            row.TransactionId,
            row.CreatedAt,
            services);
    }
}
