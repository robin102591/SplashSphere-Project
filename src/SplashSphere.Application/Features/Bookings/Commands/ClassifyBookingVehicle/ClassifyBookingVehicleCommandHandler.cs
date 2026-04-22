using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Bookings.Commands.ClassifyBookingVehicle;

public sealed class ClassifyBookingVehicleCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<ClassifyBookingVehicleCommand, Result<BookingClassificationResultDto>>
{
    public async Task<Result<BookingClassificationResultDto>> Handle(
        ClassifyBookingVehicleCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await db.Bookings
            .Include(b => b.Services)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking is null)
            return Result.Failure<BookingClassificationResultDto>(
                Error.NotFound("Booking", request.BookingId));

        if (booking.IsVehicleClassified)
            return Result.Failure<BookingClassificationResultDto>(Error.Validation(
                "BOOKING_ALREADY_CLASSIFIED",
                "This booking's vehicle has already been classified."));

        if (booking.Status is not (BookingStatus.Confirmed or BookingStatus.Arrived))
            return Result.Failure<BookingClassificationResultDto>(Error.Validation(
                "BOOKING_NOT_CLASSIFIABLE",
                $"A booking in '{booking.Status}' state cannot be classified."));

        // ── Validate VehicleType + Size exist in this tenant ─────────────────
        var vehicleTypeExists = await db.VehicleTypes
            .AnyAsync(vt => vt.Id == request.VehicleTypeId, cancellationToken);
        if (!vehicleTypeExists)
            return Result.Failure<BookingClassificationResultDto>(
                Error.NotFound("VehicleType", request.VehicleTypeId));

        var sizeExists = await db.Sizes
            .AnyAsync(s => s.Id == request.SizeId, cancellationToken);
        if (!sizeExists)
            return Result.Failure<BookingClassificationResultDto>(
                Error.NotFound("Size", request.SizeId));

        // ── Resolve the plate from the (global) ConnectVehicle ───────────────
        var vehicle = await db.ConnectVehicles
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(v => v.Id == booking.ConnectVehicleId)
            .Select(v => new { v.PlateNumber, v.Color, v.Year })
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle is null)
            return Result.Failure<BookingClassificationResultDto>(
                Error.NotFound("ConnectVehicle", booking.ConnectVehicleId));

        var plate = vehicle.PlateNumber.ToUpperInvariant().Trim();

        // ── Find / create tenant Car for this plate ──────────────────────────
        // Plate is unique per tenant; race-safe because the unique index would
        // reject a duplicate, and we explicitly re-fetch first.
        var car = await db.Cars
            .FirstOrDefaultAsync(c => c.PlateNumber == plate, cancellationToken);

        if (car is null)
        {
            car = new Car(
                tenantId: tenantContext.TenantId,
                vehicleTypeId: request.VehicleTypeId,
                sizeId: request.SizeId,
                plateNumber: plate,
                customerId: booking.CustomerId,
                color: vehicle.Color,
                year: vehicle.Year);

            db.Cars.Add(car);
        }
        else
        {
            // Another cashier / flow raced us. Adopt the existing row and
            // align the customer link if it's missing; keep whatever
            // VehicleTypeId/SizeId already exist (those are trustworthy).
            if (string.IsNullOrWhiteSpace(car.CustomerId))
                car.CustomerId = booking.CustomerId;
        }

        booking.CarId = car.Id;
        booking.IsVehicleClassified = true;

        // ── Lock exact prices on every BookingService via the matrix ─────────
        var serviceIds = booking.Services.Select(bs => bs.ServiceId).ToList();

        // Resolve service names for the response DTO (single round-trip).
        var serviceNames = await db.Services
            .AsNoTracking()
            .Where(s => serviceIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name, s.BasePrice })
            .ToListAsync(cancellationToken);

        var serviceLookup = serviceNames.ToDictionary(s => s.Id);

        // Prefer the car's VehicleTypeId/SizeId (adopted path) so we don't
        // diverge from the tenant's existing classification if they raced us.
        var effectiveVehicleTypeId = car.VehicleTypeId;
        var effectiveSizeId = car.SizeId;

        var pricingRows = await db.ServicePricings
            .AsNoTracking()
            .Where(p => serviceIds.Contains(p.ServiceId)
                     && p.VehicleTypeId == effectiveVehicleTypeId
                     && p.SizeId == effectiveSizeId)
            .Select(p => new { p.ServiceId, p.Price })
            .ToListAsync(cancellationToken);

        var priceLookup = pricingRows.ToDictionary(p => p.ServiceId, p => p.Price);

        var resultServices = new List<ClassifiedBookingServiceDto>(booking.Services.Count);
        decimal total = 0m;

        foreach (var bs in booking.Services)
        {
            var price = priceLookup.TryGetValue(bs.ServiceId, out var matrixPrice)
                ? matrixPrice
                : (serviceLookup.TryGetValue(bs.ServiceId, out var svc) ? svc.BasePrice : 0m);

            bs.Price = price;
            bs.PriceMin = null;
            bs.PriceMax = null;

            total += price;

            var serviceName = serviceLookup.TryGetValue(bs.ServiceId, out var s)
                ? s.Name
                : bs.ServiceId;

            resultServices.Add(new ClassifiedBookingServiceDto(
                bs.ServiceId, serviceName, price));
        }

        booking.EstimatedTotal = total;
        booking.EstimatedTotalMin = null;
        booking.EstimatedTotalMax = null;

        // UoWBehavior saves.
        return Result.Success(new BookingClassificationResultDto(
            booking.Id,
            car.Id,
            total,
            resultServices));
    }
}
