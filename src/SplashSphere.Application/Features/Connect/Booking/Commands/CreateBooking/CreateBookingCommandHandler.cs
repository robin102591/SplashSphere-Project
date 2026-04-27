using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Connect.Booking;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;
using DomainBookingService = SplashSphere.Domain.Entities.BookingService;

namespace SplashSphere.Application.Features.Connect.Booking.Commands.CreateBooking;

public sealed class CreateBookingCommandHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser,
    IEventPublisher eventPublisher)
    : IRequestHandler<CreateBookingCommand, Result<BookingDetailDto>>
{
    private static readonly TimeZoneInfo Manila =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

    private static readonly BookingStatus[] CountableStatuses =
    [
        BookingStatus.Confirmed,
        BookingStatus.Arrived,
        BookingStatus.InService,
    ];

    public async Task<Result<BookingDetailDto>> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated)
        {
            return Result.Failure<BookingDetailDto>(Error.Unauthorized("Sign in required."));
        }

        var userId = connectUser.ConnectUserId;
        var slotUtc = DateTime.SpecifyKind(request.SlotStartUtc, DateTimeKind.Utc);

        // Link required — the customer must have joined this tenant.
        var link = await db.ConnectUserTenantLinks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                l => l.ConnectUserId == userId && l.TenantId == request.TenantId && l.IsActive,
                cancellationToken);
        if (link is null)
        {
            return Result.Failure<BookingDetailDto>(Error.Forbidden(
                "Join this car wash before booking."));
        }

        var branch = await db.Branches
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                b => b.Id == request.BranchId
                  && b.TenantId == request.TenantId
                  && b.IsActive,
                cancellationToken);
        if (branch is null)
        {
            return Result.Failure<BookingDetailDto>(Error.NotFound("Branch", request.BranchId));
        }

        var setting = await db.BookingSettings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                s => s.TenantId == request.TenantId && s.BranchId == request.BranchId,
                cancellationToken);
        if (setting is null || !setting.IsBookingEnabled)
        {
            return Result.Failure<BookingDetailDto>(Error.Forbidden(
                "Online booking is not available at this branch."));
        }

        // Slot must line up with interval and operating hours.
        if (!IsValidSlot(slotUtc, setting))
        {
            return Result.Failure<BookingDetailDto>(Error.Validation(
                "INVALID_SLOT", "The requested time is not a valid booking slot."));
        }

        var nowUtc = DateTime.UtcNow;
        if (slotUtc < nowUtc.AddMinutes(setting.MinLeadTimeMinutes))
        {
            return Result.Failure<BookingDetailDto>(Error.Validation(
                "SLOT_TOO_SOON",
                $"Slot must be at least {setting.MinLeadTimeMinutes} minutes from now."));
        }

        var vehicle = await db.ConnectVehicles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                v => v.Id == request.ConnectVehicleId && v.ConnectUserId == userId,
                cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure<BookingDetailDto>(Error.NotFound("ConnectVehicle", request.ConnectVehicleId));
        }

        // Load and validate services.
        var services = await db.Services
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == request.TenantId
                     && s.IsActive
                     && request.ServiceIds.Contains(s.Id))
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.BasePrice,
            })
            .ToListAsync(cancellationToken);

        if (services.Count != request.ServiceIds.Count)
        {
            return Result.Failure<BookingDetailDto>(Error.Validation(
                "One or more selected services are invalid."));
        }

        // Capacity check — count CountableStatuses for this exact slot start.
        var bookedCount = await db.Bookings
            .IgnoreQueryFilters()
            .CountAsync(
                b => b.TenantId == request.TenantId
                  && b.BranchId == request.BranchId
                  && b.SlotStart == slotUtc
                  && CountableStatuses.Contains(b.Status),
                cancellationToken);
        if (bookedCount >= setting.MaxBookingsPerSlot)
        {
            return Result.Failure<BookingDetailDto>(Error.Conflict(
                "This slot has reached its capacity — please pick another time."));
        }

        // Classification check — is this vehicle already known at the tenant?
        var tenantCar = await db.Cars
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == request.TenantId
                     && c.PlateNumber == vehicle.PlateNumber)
            .Select(c => new { c.Id, c.VehicleTypeId, c.SizeId })
            .FirstOrDefaultAsync(cancellationToken);

        var serviceIds = services.Select(s => s.Id).ToList();

        var pricingRows = await db.ServicePricings
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == request.TenantId && serviceIds.Contains(p.ServiceId))
            .Select(p => new
            {
                p.ServiceId,
                p.VehicleTypeId,
                p.SizeId,
                p.Price,
            })
            .ToListAsync(cancellationToken);

        var isClassified = tenantCar is not null;

        var bookingId = Guid.NewGuid().ToString();
        var slotEndUtc = slotUtc.AddMinutes(setting.SlotIntervalMinutes);
        var booking = new Domain.Entities.Booking(
            tenantId: request.TenantId,
            branchId: request.BranchId,
            customerId: link.CustomerId,
            connectUserId: userId,
            connectVehicleId: vehicle.Id,
            slotStart: slotUtc,
            slotEnd: slotEndUtc,
            estimatedDurationMinutes: setting.SlotIntervalMinutes)
        {
            Id = bookingId,
            CarId = tenantCar?.Id,
            IsVehicleClassified = isClassified,
        };

        var serviceDtos = new List<BookingServiceDto>(services.Count);
        decimal totalExact = 0m;
        decimal totalMin = 0m;
        decimal totalMax = 0m;

        foreach (var s in services)
        {
            decimal? price = null;
            decimal? priceMin = null;
            decimal? priceMax = null;

            if (isClassified)
            {
                var match = pricingRows.FirstOrDefault(p =>
                    p.ServiceId == s.Id
                    && p.VehicleTypeId == tenantCar!.VehicleTypeId
                    && p.SizeId == tenantCar.SizeId);
                price = match?.Price ?? s.BasePrice;
                totalExact += price.Value;
            }
            else
            {
                var rowsForService = pricingRows.Where(p => p.ServiceId == s.Id).ToList();
                if (rowsForService.Count > 0)
                {
                    priceMin = rowsForService.Min(r => r.Price);
                    priceMax = rowsForService.Max(r => r.Price);
                }
                else
                {
                    priceMin = s.BasePrice;
                    priceMax = s.BasePrice;
                }
                totalMin += priceMin.Value;
                totalMax += priceMax.Value;
            }

            var line = new DomainBookingService(
                tenantId: request.TenantId,
                bookingId: bookingId,
                serviceId: s.Id)
            {
                Price = price,
                PriceMin = priceMin,
                PriceMax = priceMax,
            };
            db.BookingServices.Add(line);

            serviceDtos.Add(new BookingServiceDto(s.Id, s.Name, price, priceMin, priceMax));
        }

        if (isClassified)
        {
            booking.EstimatedTotal = totalExact;
            booking.EstimatedTotalMin = null;
            booking.EstimatedTotalMax = null;
        }
        else
        {
            booking.EstimatedTotal = (totalMin + totalMax) / 2m;
            booking.EstimatedTotalMin = totalMin;
            booking.EstimatedTotalMax = totalMax;
        }

        db.Bookings.Add(booking);

        // Emit a BookingConfirmedEvent after the UoW flush so downstream
        // notification handlers see the persisted booking.
        eventPublisher.Enqueue(new BookingConfirmedEvent(
            BookingId: booking.Id,
            TenantId: booking.TenantId,
            BranchId: booking.BranchId,
            CustomerId: booking.CustomerId,
            PlateNumber: vehicle.PlateNumber,
            SlotStartUtc: booking.SlotStart,
            SlotEndUtc: booking.SlotEnd,
            VehicleLabel: null));

        // UoWBehavior saves.
        return Result.Success(new BookingDetailDto(
            booking.Id,
            booking.TenantId,
            TenantName: string.Empty, // hydrated by subsequent read when the UI needs it
            booking.BranchId,
            BranchName: branch.Name,
            booking.SlotStart,
            booking.SlotEnd,
            Status: booking.Status.ToString(),
            booking.IsVehicleClassified,
            booking.EstimatedTotal,
            booking.EstimatedTotalMin,
            booking.EstimatedTotalMax,
            booking.EstimatedDurationMinutes,
            booking.ConnectVehicleId,
            vehicle.PlateNumber,
            QueueEntryId: null,
            QueueNumber: null,
            QueueStatus: null,
            TransactionId: null,
            Services: serviceDtos));
    }

    private static bool IsValidSlot(DateTime slotUtc, Domain.Entities.BookingSetting setting)
    {
        var slotManila = TimeZoneInfo.ConvertTimeFromUtc(slotUtc, Manila);
        var dateOnly = DateOnly.FromDateTime(slotManila);
        var time = TimeOnly.FromDateTime(slotManila);

        if (time < setting.OpenTime || time >= setting.CloseTime) return false;

        var minutesFromOpen = (int)(time - setting.OpenTime).TotalMinutes;
        if (minutesFromOpen < 0) return false;
        if (minutesFromOpen % setting.SlotIntervalMinutes != 0) return false;

        // Slot must fit entirely inside operating hours.
        var slotEnd = slotManila.AddMinutes(setting.SlotIntervalMinutes);
        var closeDateTime = dateOnly.ToDateTime(setting.CloseTime);
        return slotEnd <= closeDateTime;
    }
}
