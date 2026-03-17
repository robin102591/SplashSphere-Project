using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Transactions.Commands.CreateTransaction;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;
using MerchandiseEntity = SplashSphere.Domain.Entities.Merchandise;

namespace SplashSphere.Application.Features.Transactions.Commands.UpdateTransactionItems;

public sealed class UpdateTransactionItemsCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IEventPublisher eventPublisher)
    : IRequestHandler<UpdateTransactionItemsCommand, Result>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<Result> Handle(
        UpdateTransactionItemsCommand request,
        CancellationToken cancellationToken)
    {
        // ── Guard: transaction must be InProgress ─────────────────────────────
        var transaction = await context.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
            return Result.Failure(Error.NotFound("Transaction", request.TransactionId));

        if (transaction.Status != TransactionStatus.InProgress)
            return Result.Failure(Error.Validation(
                $"Items can only be updated on InProgress transactions. Current status: '{transaction.Status}'."));

        // ── Guard: no payments yet ────────────────────────────────────────────
        var hasPayments = await context.Payments
            .AnyAsync(p => p.TransactionId == request.TransactionId, cancellationToken);

        if (hasPayments)
            return Result.Failure(Error.Validation(
                "Items cannot be updated after a payment has been recorded. " +
                "Cancel the transaction and create a new one to change items."));

        // ── Load branch + car ─────────────────────────────────────────────────
        var branch = await context.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == transaction.BranchId, cancellationToken);

        if (branch is null)
            return Result.Failure(Error.NotFound("Branch", transaction.BranchId));

        var car = await context.Cars
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == transaction.CarId, cancellationToken);

        if (car is null)
            return Result.Failure(Error.NotFound("Car", transaction.CarId));

        var vehicleTypeId = car.VehicleTypeId;
        var sizeId = car.SizeId;

        // ── Validate requested IDs ────────────────────────────────────────────
        var serviceIds = request.Services.Select(s => s.ServiceId).ToList();
        var services = await context.Services
            .AsNoTracking()
            .Where(s => serviceIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        if (services.Count != serviceIds.Count)
        {
            var missing = serviceIds.Except(services.Select(s => s.Id)).First();
            return Result.Failure(Error.NotFound("Service", missing));
        }

        var inactiveService = services.FirstOrDefault(s => !s.IsActive);
        if (inactiveService is not null)
            return Result.Failure(Error.Validation($"Service '{inactiveService.Name}' is not active."));

        var packageIds = request.Packages.Select(p => p.PackageId).ToList();
        var packages = await context.ServicePackages
            .AsNoTracking()
            .Where(p => packageIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (packages.Count != packageIds.Count)
        {
            var missing = packageIds.Except(packages.Select(p => p.Id)).First();
            return Result.Failure(Error.NotFound("Package", missing));
        }

        var inactivePackage = packages.FirstOrDefault(p => !p.IsActive);
        if (inactivePackage is not null)
            return Result.Failure(Error.Validation($"Package '{inactivePackage.Name}' is not active."));

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
                return Result.Failure(Error.NotFound("Employee", missing));
            }

            var inactiveEmployee = employees.FirstOrDefault(e => !e.IsActive);
            if (inactiveEmployee is not null)
                return Result.Failure(Error.Validation($"Employee '{inactiveEmployee.FullName}' is not active."));

            employeeMap = employees.ToDictionary(e => e.Id);
        }

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
                return Result.Failure(Error.NotFound("Merchandise", missing));
            }

            merchandiseMap = merchandiseItems.ToDictionary<MerchandiseEntity, string>(m => m.Id);
        }

        // ── Restore stock for existing merchandise lines ───────────────────────
        var existingMerchandise = await context.TransactionMerchandise
            .Where(tm => tm.TransactionId == request.TransactionId)
            .ToListAsync(cancellationToken);

        foreach (var tm in existingMerchandise)
        {
            var merch = await context.Merchandise
                .FirstOrDefaultAsync(m => m.Id == tm.MerchandiseId, cancellationToken);
            if (merch is not null)
                merch.StockQuantity += tm.Quantity;
        }

        // ── Stock check for new merchandise ───────────────────────────────────
        foreach (var merchRequest in request.Merchandise)
        {
            var merch = merchandiseMap[merchRequest.MerchandiseId];
            // Stock was already restored above; check against restored quantity
            if (merch.StockQuantity < merchRequest.Quantity)
                return Result.Failure(Error.Validation(
                    $"Insufficient stock for '{merch.Name}': " +
                    $"requested {merchRequest.Quantity}, available {merch.StockQuantity}."));
        }

        // ── Load pricing data ─────────────────────────────────────────────────
        var manilaNow = DateTime.UtcNow + ManilaOffset;
        var manilaDate = DateOnly.FromDateTime(manilaNow);
        var manilaTime = TimeOnly.FromDateTime(manilaNow);

        var servicePricingRows = await context.ServicePricings
            .AsNoTracking()
            .Where(sp => serviceIds.Contains(sp.ServiceId)
                      && sp.VehicleTypeId == vehicleTypeId
                      && sp.SizeId == sizeId)
            .ToListAsync(cancellationToken);

        var servicePricingMap = servicePricingRows.ToDictionary(sp => sp.ServiceId);

        var serviceCommissionRows = await context.ServiceCommissions
            .AsNoTracking()
            .Where(sc => serviceIds.Contains(sc.ServiceId)
                      && sc.VehicleTypeId == vehicleTypeId
                      && sc.SizeId == sizeId)
            .ToListAsync(cancellationToken);

        var serviceCommissionMap = serviceCommissionRows.ToDictionary(sc => sc.ServiceId);

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

        var activeModifiers = await context.PricingModifiers
            .AsNoTracking()
            .Where(m => m.IsActive && (m.BranchId == null || m.BranchId == transaction.BranchId))
            .ToListAsync(cancellationToken);

        var effectiveModifiers = activeModifiers
            .Where(m => IsModifierCurrentlyActive(m, manilaDate, manilaTime))
            .ToList();

        // ── Remove existing line items ────────────────────────────────────────
        var existingServices = await context.TransactionServices
            .Where(ts => ts.TransactionId == request.TransactionId)
            .ToListAsync(cancellationToken);
        context.TransactionServices.RemoveRange(existingServices);

        var existingPackages = await context.TransactionPackages
            .Where(tp => tp.TransactionId == request.TransactionId)
            .ToListAsync(cancellationToken);
        context.TransactionPackages.RemoveRange(existingPackages);

        context.TransactionMerchandise.RemoveRange(existingMerchandise);

        var existingEmployees = await context.TransactionEmployees
            .Where(te => te.TransactionId == request.TransactionId)
            .ToListAsync(cancellationToken);
        context.TransactionEmployees.RemoveRange(existingEmployees);

        // ── Build new service line items ───────────────────────────────────────
        var employeeCommissions = new Dictionary<string, decimal>();
        decimal totalServiceAmount = 0;

        foreach (var svcRequest in request.Services)
        {
            var service = services.First(s => s.Id == svcRequest.ServiceId);
            var basePrice = servicePricingMap.TryGetValue(svcRequest.ServiceId, out var pricingRow)
                ? pricingRow.Price
                : service.BasePrice;
            var finalPrice = ApplyModifiers(basePrice, effectiveModifiers);

            var totalCommission = CalculateServiceCommission(finalPrice, svcRequest.ServiceId, serviceCommissionMap);
            var commissionPerEmployee = svcRequest.EmployeeIds.Count > 0
                ? Math.Round(totalCommission / svcRequest.EmployeeIds.Count, 2, MidpointRounding.AwayFromZero)
                : 0m;

            var txService = new TransactionService(
                tenantContext.TenantId,
                request.TransactionId,
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

            context.TransactionServices.Add(txService);
            totalServiceAmount += finalPrice;
        }

        // ── Build new package line items ───────────────────────────────────────
        decimal totalPackageAmount = 0;

        foreach (var pkgRequest in request.Packages)
        {
            var packagePrice = packagePricingMap.TryGetValue(pkgRequest.PackageId, out var pkgPricingRow)
                ? pkgPricingRow.Price
                : 0m;
            var finalPackagePrice = ApplyModifiers(packagePrice, effectiveModifiers);

            var pkgTotalCommission = packageCommissionMap.TryGetValue(pkgRequest.PackageId, out var pkgCommRow)
                ? Math.Round(finalPackagePrice * pkgCommRow.PercentageRate / 100, 2, MidpointRounding.AwayFromZero)
                : 0m;

            var commissionPerEmployee = pkgRequest.EmployeeIds.Count > 0
                ? Math.Round(pkgTotalCommission / pkgRequest.EmployeeIds.Count, 2, MidpointRounding.AwayFromZero)
                : 0m;

            var txPackage = new TransactionPackage(
                tenantContext.TenantId,
                request.TransactionId,
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

            context.TransactionPackages.Add(txPackage);
            totalPackageAmount += finalPackagePrice;
        }

        // ── Build new merchandise lines ────────────────────────────────────────
        decimal totalMerchandiseAmount = 0;

        foreach (var merchRequest in request.Merchandise)
        {
            var merch = merchandiseMap[merchRequest.MerchandiseId];
            merch.StockQuantity -= merchRequest.Quantity;

            var txMerch = new TransactionMerchandise(
                tenantContext.TenantId,
                request.TransactionId,
                merchRequest.MerchandiseId,
                merchRequest.Quantity,
                merch.Price);

            context.TransactionMerchandise.Add(txMerch);
            totalMerchandiseAmount += txMerch.LineTotal;
        }

        // ── Rebuild employee summaries ─────────────────────────────────────────
        foreach (var (employeeId, totalCommission) in employeeCommissions)
        {
            context.TransactionEmployees.Add(new TransactionEmployee(
                tenantContext.TenantId,
                request.TransactionId,
                employeeId,
                totalCommission));
        }

        // ── Update transaction totals ──────────────────────────────────────────
        transaction.TotalAmount    = totalServiceAmount + totalPackageAmount + totalMerchandiseAmount;
        transaction.DiscountAmount = request.DiscountAmount;
        transaction.FinalAmount    = transaction.TotalAmount - request.DiscountAmount + transaction.TaxAmount;
        transaction.Notes          = request.Notes;

        eventPublisher.Enqueue(new TransactionUpdatedEvent(
            transaction.Id,
            tenantContext.TenantId,
            transaction.BranchId,
            transaction.FinalAmount));

        return Result.Success();
    }

    private static bool IsModifierCurrentlyActive(PricingModifier modifier, DateOnly manilaDate, TimeOnly manilaTime)
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
            ModifierType.Weather => true,
            _ => false,
        };

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
                Math.Round(commission.FixedAmount!.Value + (price * commission.PercentageRate!.Value / 100), 2, MidpointRounding.AwayFromZero),
            _ => 0m,
        };
    }
}
