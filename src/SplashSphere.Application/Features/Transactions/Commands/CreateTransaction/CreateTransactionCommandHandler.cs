using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;
using MerchandiseEntity = SplashSphere.Domain.Entities.Merchandise;

namespace SplashSphere.Application.Features.Transactions.Commands.CreateTransaction;

public sealed class CreateTransactionCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IEventPublisher eventPublisher)
    : IRequestHandler<CreateTransactionCommand, Result<string>>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<Result<string>> Handle(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // ── Step 1: Validate all IDs ──────────────────────────────────────────

        var branch = await context.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BranchId, cancellationToken);

        if (branch is null)
            return Result.Failure<string>(Error.NotFound("Branch", request.BranchId));

        if (!branch.IsActive)
            return Result.Failure<string>(Error.Validation("Branch is not active."));

        // ── Car resolution: use CarId if provided, otherwise look up by plate
        //    and auto-create a minimal record (VehicleTypeId + SizeId required). ──────
        Car? car;
        if (!string.IsNullOrWhiteSpace(request.CarId))
        {
            car = await context.Cars
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CarId, cancellationToken);

            if (car is null)
                return Result.Failure<string>(Error.NotFound("Car", request.CarId));
        }
        else
        {
            var plate = request.PlateNumber!.ToUpperInvariant().Trim();

            car = await context.Cars
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.PlateNumber == plate, cancellationToken);

            if (car is null)
            {
                // Auto-create a minimal walk-in car record.
                var newCar = new Car(
                    tenantContext.TenantId,
                    request.VehicleTypeId!,
                    request.SizeId!,
                    plate,
                    request.CustomerId);

                context.Cars.Add(newCar);
                car = newCar;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerId))
        {
            var customerExists = await context.Customers
                .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (!customerExists)
                return Result.Failure<string>(Error.NotFound("Customer", request.CustomerId));
        }

        QueueEntry? queueEntry = null;
        if (!string.IsNullOrWhiteSpace(request.QueueEntryId))
        {
            queueEntry = await context.QueueEntries
                .FirstOrDefaultAsync(q => q.Id == request.QueueEntryId, cancellationToken);

            if (queueEntry is null)
                return Result.Failure<string>(Error.NotFound("QueueEntry", request.QueueEntryId));

            if (queueEntry.Status != QueueStatus.Called)
                return Result.Failure<string>(Error.Validation(
                    $"Queue entry is in status '{queueEntry.Status}' and cannot be started. " +
                    "Only CALLED entries can be transitioned to IN_SERVICE."));
        }

        // Load all requested service entities
        var serviceIds = request.Services.Select(s => s.ServiceId).ToList();
        var services = await context.Services
            .AsNoTracking()
            .Where(s => serviceIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        if (services.Count != serviceIds.Count)
        {
            var missing = serviceIds.Except(services.Select(s => s.Id)).First();
            return Result.Failure<string>(Error.NotFound("Service", missing));
        }

        var inactiveService = services.FirstOrDefault(s => !s.IsActive);
        if (inactiveService is not null)
            return Result.Failure<string>(Error.Validation(
                $"Service '{inactiveService.Name}' is not active."));

        // Load all requested package entities
        var packageIds = request.Packages.Select(p => p.PackageId).ToList();
        var packages = await context.ServicePackages
            .AsNoTracking()
            .Where(p => packageIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (packages.Count != packageIds.Count)
        {
            var missing = packageIds.Except(packages.Select(p => p.Id)).First();
            return Result.Failure<string>(Error.NotFound("Package", missing));
        }

        var inactivePackage = packages.FirstOrDefault(p => !p.IsActive);
        if (inactivePackage is not null)
            return Result.Failure<string>(Error.Validation(
                $"Package '{inactivePackage.Name}' is not active."));

        // Collect all employee IDs across services and packages
        var allEmployeeIds = request.Services
            .SelectMany(s => s.EmployeeIds)
            .Concat(request.Packages.SelectMany(p => p.EmployeeIds))
            .Distinct()
            .ToList();

        Dictionary<string, Employee> employeeMap = [];
        if (allEmployeeIds.Count > 0)
        {
            var employees = await context.Employees
                .AsNoTracking()
                .Where(e => allEmployeeIds.Contains(e.Id))
                .ToListAsync(cancellationToken);

            if (employees.Count != allEmployeeIds.Count)
            {
                var missing = allEmployeeIds.Except(employees.Select(e => e.Id)).First();
                return Result.Failure<string>(Error.NotFound("Employee", missing));
            }

            var inactiveEmployee = employees.FirstOrDefault(e => !e.IsActive);
            if (inactiveEmployee is not null)
                return Result.Failure<string>(Error.Validation(
                    $"Employee '{inactiveEmployee.FullName}' is not active."));

            employeeMap = employees.ToDictionary(e => e.Id);
        }

        // Load merchandise
        var merchandiseIds = request.Merchandise.Select(m => m.MerchandiseId).ToList();
        Dictionary<string, MerchandiseEntity> merchandiseMap = [];
        if (merchandiseIds.Count > 0)
        {
            var merchandiseItems = await context.Merchandise
                .Where(m => merchandiseIds.Contains(m.Id))
                .ToListAsync(cancellationToken);

            if (merchandiseItems.Count != merchandiseIds.Count)
            {
                var missing = merchandiseIds.Except(merchandiseItems.Select(m => m.Id)).First();
                return Result.Failure<string>(Error.NotFound("Merchandise", missing));
            }

            merchandiseMap = merchandiseItems.ToDictionary<MerchandiseEntity, string>(m => m.Id);
        }

        // ── Step 2 & 3: Service pricing + commission ──────────────────────────

        var vehicleTypeId = car.VehicleTypeId;
        var sizeId = car.SizeId;
        var manilaNow = DateTime.UtcNow + ManilaOffset;
        var manilaDate = DateOnly.FromDateTime(manilaNow);
        var manilaTime = TimeOnly.FromDateTime(manilaNow);

        // Load service pricing matrix rows for this vehicle type + size
        var servicePricingRows = await context.ServicePricings
            .AsNoTracking()
            .Where(sp => serviceIds.Contains(sp.ServiceId)
                      && sp.VehicleTypeId == vehicleTypeId
                      && sp.SizeId == sizeId)
            .ToListAsync(cancellationToken);

        var servicePricingMap = servicePricingRows.ToDictionary(sp => sp.ServiceId);

        // Load service commission matrix rows for this vehicle type + size
        var serviceCommissionRows = await context.ServiceCommissions
            .AsNoTracking()
            .Where(sc => serviceIds.Contains(sc.ServiceId)
                      && sc.VehicleTypeId == vehicleTypeId
                      && sc.SizeId == sizeId)
            .ToListAsync(cancellationToken);

        var serviceCommissionMap = serviceCommissionRows.ToDictionary(sc => sc.ServiceId);

        // ── Step 4: Package pricing + commission ──────────────────────────────

        var packagePricingRows = await context.PackagePricings
            .AsNoTracking()
            .Where(pp => packageIds.Contains(pp.PackageId)
                      && pp.VehicleTypeId == vehicleTypeId
                      && pp.SizeId == sizeId)
            .ToListAsync(cancellationToken);

        var packagePricingMap = packagePricingRows.ToDictionary(pp => pp.PackageId);

        var packageCommissionRows = await context.PackageCommissions
            .AsNoTracking()
            .Where(pc => packageIds.Contains(pc.PackageId)
                      && pc.VehicleTypeId == vehicleTypeId
                      && pc.SizeId == sizeId)
            .ToListAsync(cancellationToken);

        var packageCommissionMap = packageCommissionRows.ToDictionary(pc => pc.PackageId);

        // Load active pricing modifiers for this branch (branch-specific OR tenant-wide)
        var activeModifiers = await context.PricingModifiers
            .AsNoTracking()
            .Where(m => m.IsActive && (m.BranchId == null || m.BranchId == request.BranchId))
            .ToListAsync(cancellationToken);

        var effectiveModifiers = activeModifiers.Where(m => IsModifierCurrentlyActive(m, manilaDate, manilaTime)).ToList();

        // ── Step 5: Merchandise stock check ───────────────────────────────────

        foreach (var merchRequest in request.Merchandise)
        {
            var merch = merchandiseMap[merchRequest.MerchandiseId];
            if (merch.StockQuantity < merchRequest.Quantity)
                return Result.Failure<string>(Error.Validation(
                    $"Insufficient stock for '{merch.Name}': " +
                    $"requested {merchRequest.Quantity}, available {merch.StockQuantity}."));
        }

        // ── Step 8: Generate transaction number ───────────────────────────────
        // Count existing transactions for this branch on the Manila calendar date.
        var todayStartUtc = DateTime.SpecifyKind(manilaDate.ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var todayEndUtc   = todayStartUtc.AddDays(1);

        var dailyCount = await context.Transactions
            .CountAsync(t => t.BranchId == request.BranchId
                          && t.CreatedAt >= todayStartUtc
                          && t.CreatedAt < todayEndUtc,
                        cancellationToken);

        var sequence = dailyCount + 1;
        var transactionNumber = $"{branch.Code}-{manilaDate:yyyyMMdd}-{sequence:D4}";

        // ── Build transaction entity ──────────────────────────────────────────
        var transactionId = Ulid.NewUlid().ToString();
        var transaction = new Transaction(
            transactionId,
            tenantContext.TenantId,
            request.BranchId,
            tenantContext.UserId,
            car.Id,
            request.CustomerId)
        {
            TransactionNumber = transactionNumber,
            DiscountAmount    = request.DiscountAmount,
            TaxAmount         = request.TaxAmount,
            Notes             = request.Notes,
        };

        // ── Build service line items (Steps 2 & 3) ────────────────────────────

        // Per-employee commission accumulator for Step 7
        var employeeCommissions = new Dictionary<string, decimal>();

        decimal totalServiceAmount = 0;

        foreach (var svcRequest in request.Services)
        {
            var service = services.First(s => s.Id == svcRequest.ServiceId);

            // Pricing: matrix row → fallback to BasePrice → apply modifiers
            var basePrice = servicePricingMap.TryGetValue(svcRequest.ServiceId, out var pricingRow)
                ? pricingRow.Price
                : service.BasePrice;

            var finalPrice = ApplyModifiers(basePrice, effectiveModifiers);

            // Commission
            var totalCommission = CalculateServiceCommission(
                finalPrice,
                svcRequest.ServiceId,
                serviceCommissionMap);

            var commissionPerEmployee = svcRequest.EmployeeIds.Count > 0
                ? Math.Round(totalCommission / svcRequest.EmployeeIds.Count, 2, MidpointRounding.AwayFromZero)
                : 0m;

            var txService = new TransactionService(
                tenantContext.TenantId,
                transactionId,
                svcRequest.ServiceId,
                vehicleTypeId,
                sizeId,
                finalPrice,
                totalCommission);

            foreach (var employeeId in svcRequest.EmployeeIds)
            {
                txService.EmployeeAssignments.Add(new ServiceEmployeeAssignment(
                    tenantContext.TenantId,
                    txService.Id,
                    employeeId,
                    commissionPerEmployee));

                employeeCommissions[employeeId] =
                    employeeCommissions.GetValueOrDefault(employeeId) + commissionPerEmployee;
            }

            transaction.Services.Add(txService);
            totalServiceAmount += finalPrice;
        }

        // ── Build package line items (Step 4) ─────────────────────────────────

        decimal totalPackageAmount = 0;

        foreach (var pkgRequest in request.Packages)
        {
            // Pricing: matrix row → no BasePrice fallback for packages (₱0 if missing)
            var packagePrice = packagePricingMap.TryGetValue(pkgRequest.PackageId, out var pkgPricingRow)
                ? pkgPricingRow.Price
                : 0m;

            var finalPackagePrice = ApplyModifiers(packagePrice, effectiveModifiers);

            // Commission: always percentage
            var pkgTotalCommission = packageCommissionMap.TryGetValue(pkgRequest.PackageId, out var pkgCommRow)
                ? Math.Round(finalPackagePrice * pkgCommRow.PercentageRate / 100, 2, MidpointRounding.AwayFromZero)
                : 0m;

            var commissionPerEmployee = pkgRequest.EmployeeIds.Count > 0
                ? Math.Round(pkgTotalCommission / pkgRequest.EmployeeIds.Count, 2, MidpointRounding.AwayFromZero)
                : 0m;

            var txPackage = new TransactionPackage(
                tenantContext.TenantId,
                transactionId,
                pkgRequest.PackageId,
                vehicleTypeId,
                sizeId,
                finalPackagePrice,
                pkgTotalCommission);

            foreach (var employeeId in pkgRequest.EmployeeIds)
            {
                txPackage.EmployeeAssignments.Add(new PackageEmployeeAssignment(
                    tenantContext.TenantId,
                    txPackage.Id,
                    employeeId,
                    commissionPerEmployee));

                employeeCommissions[employeeId] =
                    employeeCommissions.GetValueOrDefault(employeeId) + commissionPerEmployee;
            }

            transaction.Packages.Add(txPackage);
            totalPackageAmount += finalPackagePrice;
        }

        // ── Step 5: Merchandise inventory decrement ───────────────────────────

        decimal totalMerchandiseAmount = 0;

        foreach (var merchRequest in request.Merchandise)
        {
            var merch = merchandiseMap[merchRequest.MerchandiseId];
            merch.StockQuantity -= merchRequest.Quantity;

            var txMerch = new TransactionMerchandise(
                tenantContext.TenantId,
                transactionId,
                merchRequest.MerchandiseId,
                merchRequest.Quantity,
                merch.Price);

            transaction.Merchandise.Add(txMerch);
            totalMerchandiseAmount += txMerch.LineTotal;
        }

        // ── Step 6: Aggregate amounts ─────────────────────────────────────────

        transaction.TotalAmount  = totalServiceAmount + totalPackageAmount + totalMerchandiseAmount;
        transaction.FinalAmount  = transaction.TotalAmount - request.DiscountAmount + request.TaxAmount;
        transaction.TipAmount    = request.TipAmount;

        // ── Step 7: TransactionEmployee summary records ───────────────────────

        foreach (var (employeeId, totalCommission) in employeeCommissions)
        {
            transaction.Employees.Add(new TransactionEmployee(
                tenantContext.TenantId,
                transactionId,
                employeeId,
                totalCommission));
        }

        // ── Step 9: Persist and link queue entry ──────────────────────────────

        // Service begins when the transaction is created at the POS.
        // This supports both payment scenarios:
        //   • Pay later (Scenario 1): stays InProgress until AddPayment auto-completes it.
        //   • Pay now   (Scenario 2): AddPayment receives InProgress, auto-completes immediately.
        transaction.Status = TransactionStatus.InProgress;

        context.Transactions.Add(transaction);

        var now = DateTime.UtcNow;

        string? linkedQueueEntryId;

        if (queueEntry is not null)
        {
            // Queue-first workflow: transition the existing called entry to InService.
            queueEntry.Status        = QueueStatus.InService;
            queueEntry.TransactionId = transaction.Id;
            queueEntry.StartedAt     = now;
            linkedQueueEntryId       = queueEntry.Id;

            eventPublisher.Enqueue(new QueueEntryInServiceEvent(
                queueEntry.Id,
                queueEntry.TenantId,
                queueEntry.BranchId,
                queueEntry.QueueNumber,
                queueEntry.PlateNumber,
                now));
        }
        else
        {
            // Walk-in workflow: auto-create a queue entry so the vehicle appears
            // in the InService column on the queue board.
            var walkInNumbers = await context.QueueEntries
                .Where(q => q.BranchId == request.BranchId && q.QueueDate == manilaDate)
                .Select(q => q.QueueNumber)
                .ToListAsync(cancellationToken);

            var walkInMaxSeq = walkInNumbers
                .Select(n => n.StartsWith("Q-") && int.TryParse(n[2..], out var s) ? s : 0)
                .DefaultIfEmpty(0)
                .Max();

            var walkInEntry = new QueueEntry(
                tenantContext.TenantId,
                request.BranchId,
                $"Q-{walkInMaxSeq + 1:D3}",
                manilaDate,
                car.PlateNumber,
                customerId: request.CustomerId,
                carId: car.Id);

            walkInEntry.Status        = QueueStatus.InService;
            walkInEntry.TransactionId = transaction.Id;
            walkInEntry.StartedAt     = now;

            context.QueueEntries.Add(walkInEntry);
            linkedQueueEntryId = walkInEntry.Id;

            eventPublisher.Enqueue(new QueueEntryInServiceEvent(
                walkInEntry.Id,
                walkInEntry.TenantId,
                walkInEntry.BranchId,
                walkInEntry.QueueNumber,
                walkInEntry.PlateNumber,
                now));
        }

        // Publish event — SignalR hub, dashboard metrics, queue board will handle it
        eventPublisher.Enqueue(new TransactionCreatedEvent(
            transaction.Id,
            tenantContext.TenantId,
            request.BranchId,
            transaction.TransactionNumber,
            transaction.CarId,
            transaction.FinalAmount,
            transaction.Status,
            transaction.CustomerId,
            linkedQueueEntryId));

        return Result.Success(transaction.Id);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Determines whether a pricing modifier is currently active based on Manila time.
    /// Assumes the modifier's <see cref="PricingModifier.IsActive"/> flag has already
    /// been checked before calling this method.
    /// </summary>
    private static bool IsModifierCurrentlyActive(
        PricingModifier modifier,
        DateOnly manilaDate,
        TimeOnly manilaTime)
        => modifier.Type switch
        {
            ModifierType.PeakHour =>
                modifier.StartTime.HasValue && modifier.EndTime.HasValue &&
                manilaTime >= modifier.StartTime.Value && manilaTime <= modifier.EndTime.Value,

            ModifierType.DayOfWeek =>
                modifier.ActiveDayOfWeek.HasValue &&
                modifier.ActiveDayOfWeek.Value == manilaDate.DayOfWeek,

            ModifierType.Holiday =>
                modifier.HolidayDate.HasValue && modifier.HolidayDate.Value == manilaDate,

            ModifierType.Promotion =>
                modifier.StartDate.HasValue && modifier.EndDate.HasValue &&
                manilaDate >= modifier.StartDate.Value && manilaDate <= modifier.EndDate.Value,

            ModifierType.Weather =>
                true, // Weather modifiers are manually toggled via IsActive — already filtered

            _ => false,
        };

    /// <summary>
    /// Applies all active pricing modifiers to a base price.
    /// Multiplier-type modifiers stack multiplicatively.
    /// Promotion modifiers subtract an absolute PHP value.
    /// </summary>
    private static decimal ApplyModifiers(decimal basePrice, IEnumerable<PricingModifier> modifiers)
    {
        var multiplier = 1m;
        var promotionDeduction = 0m;

        foreach (var modifier in modifiers)
        {
            if (modifier.Type == ModifierType.Promotion)
                promotionDeduction += modifier.Value;
            else
                multiplier *= modifier.Value;
        }

        return Math.Max(0m, basePrice * multiplier - promotionDeduction);
    }

    /// <summary>
    /// Calculates the total commission for a service line item using the
    /// Percentage / FixedAmount / Hybrid formula.
    /// Returns ₱0 when no commission matrix row exists for the combination.
    /// </summary>
    private static decimal CalculateServiceCommission(
        decimal price,
        string serviceId,
        Dictionary<string, ServiceCommission> commissionMap)
    {
        if (!commissionMap.TryGetValue(serviceId, out var commission))
            return 0m;

        return commission.Type switch
        {
            CommissionType.Percentage =>
                Math.Round(price * commission.PercentageRate!.Value / 100, 2, MidpointRounding.AwayFromZero),

            CommissionType.FixedAmount =>
                commission.FixedAmount!.Value,

            CommissionType.Hybrid =>
                Math.Round(
                    commission.FixedAmount!.Value + (price * commission.PercentageRate!.Value / 100),
                    2, MidpointRounding.AwayFromZero),

            _ => 0m,
        };
    }
}
