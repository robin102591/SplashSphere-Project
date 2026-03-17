using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Transactions.Queries.GetTransactionById;

public sealed class GetTransactionByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTransactionByIdQuery, TransactionDetailDto?>
{
    public async Task<TransactionDetailDto?> Handle(
        GetTransactionByIdQuery request,
        CancellationToken cancellationToken)
    {
        // ── Query 1: transaction scalar fields + single-row navigations ────────
        var tx = await context.Transactions
            .AsNoTracking()
            .Where(t => t.Id == request.TransactionId)
            .Select(t => new
            {
                t.Id,
                t.TransactionNumber,
                t.BranchId,
                BranchName      = t.Branch.Name,
                t.CarId,
                PlateNumber     = t.Car.PlateNumber,
                VehicleTypeName = t.Car.VehicleType.Name,
                VehicleTypeId   = t.Car.VehicleTypeId,
                SizeName        = t.Car.Size.Name,
                SizeId          = t.Car.SizeId,
                t.CustomerId,
                CustomerName    = t.Customer != null
                    ? t.Customer.FirstName + " " + t.Customer.LastName
                    : (string?)null,
                t.Status,
                t.TotalAmount,
                t.DiscountAmount,
                t.TaxAmount,
                t.FinalAmount,
                t.CashierId,
                CashierName  = t.Cashier.FirstName + " " + t.Cashier.LastName,
                t.Notes,
                QueueEntryId = t.QueueEntry != null ? t.QueueEntry.Id : (string?)null,
                t.CreatedAt,
                t.CompletedAt,
                t.CancelledAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (tx is null)
            return null;

        // ── Query 2: service line items + employee assignments ────────────────
        var services = await context.TransactionServices
            .AsNoTracking()
            .Where(ts => ts.TransactionId == request.TransactionId)
            .Select(ts => new TransactionServiceLineDto(
                ts.Id,
                ts.ServiceId,
                ts.Service.Name,
                ts.Service.Category.Name,
                ts.VehicleType.Name,
                ts.Size.Name,
                ts.UnitPrice,
                ts.TotalCommission,
                ts.Notes,
                ts.EmployeeAssignments
                    .Select(a => new ServiceAssignmentDto(
                        a.Id,
                        a.EmployeeId,
                        a.Employee.FirstName + " " + a.Employee.LastName,
                        a.CommissionAmount))
                    .ToList()))
            .ToListAsync(cancellationToken);

        // ── Query 3: package line items + employee assignments ────────────────
        var packages = await context.TransactionPackages
            .AsNoTracking()
            .Where(tp => tp.TransactionId == request.TransactionId)
            .Select(tp => new TransactionPackageLineDto(
                tp.Id,
                tp.PackageId,
                tp.Package.Name,
                tp.VehicleType.Name,
                tp.Size.Name,
                tp.UnitPrice,
                tp.TotalCommission,
                tp.Notes,
                tp.EmployeeAssignments
                    .Select(a => new PackageAssignmentDto(
                        a.Id,
                        a.EmployeeId,
                        a.Employee.FirstName + " " + a.Employee.LastName,
                        a.CommissionAmount))
                    .ToList()))
            .ToListAsync(cancellationToken);

        // ── Query 4: merchandise + employee summaries + payments ──────────────
        var merchandise = await context.TransactionMerchandise
            .AsNoTracking()
            .Where(tm => tm.TransactionId == request.TransactionId)
            .Select(tm => new TransactionMerchandiseLineDto(
                tm.Id,
                tm.MerchandiseId,
                tm.Merchandise.Name,
                tm.Quantity,
                tm.UnitPrice,
                tm.Quantity * tm.UnitPrice))
            .ToListAsync(cancellationToken);

        var employees = await context.TransactionEmployees
            .AsNoTracking()
            .Where(te => te.TransactionId == request.TransactionId)
            .Select(te => new TransactionEmployeeSummaryDto(
                te.Id,
                te.EmployeeId,
                te.Employee.FirstName + " " + te.Employee.LastName,
                te.TotalCommission))
            .ToListAsync(cancellationToken);

        var payments = await context.Payments
            .AsNoTracking()
            .Where(p => p.TransactionId == request.TransactionId)
            .OrderBy(p => p.PaidAt)
            .Select(p => new PaymentDto(
                p.Id,
                p.PaymentMethod,
                p.Amount,
                p.ReferenceNumber,
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        return new TransactionDetailDto(
            tx.Id,
            tx.TransactionNumber,
            tx.BranchId,
            tx.BranchName,
            tx.CarId,
            tx.PlateNumber,
            tx.VehicleTypeName,
            tx.VehicleTypeId,
            tx.SizeName,
            tx.SizeId,
            tx.CustomerId,
            tx.CustomerName,
            tx.Status,
            tx.TotalAmount,
            tx.DiscountAmount,
            tx.TaxAmount,
            tx.FinalAmount,
            tx.CashierId,
            tx.CashierName,
            tx.Notes,
            tx.QueueEntryId,
            tx.CreatedAt,
            tx.CompletedAt,
            tx.CancelledAt,
            services,
            packages,
            merchandise,
            employees,
            payments);
    }
}
