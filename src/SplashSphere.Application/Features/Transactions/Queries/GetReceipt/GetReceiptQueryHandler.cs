using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Application.Features.Transactions.Queries.GetReceipt;

public sealed class GetReceiptQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReceiptQuery, ReceiptDto?>
{
    public async Task<ReceiptDto?> Handle(
        GetReceiptQuery request,
        CancellationToken cancellationToken)
    {
        // ── Query 1: header + tenant branding ─────────────────────────────────
        // The transaction has a global tenant filter; the projection pulls
        // the owning Tenant's display fields so the renderer doesn't need
        // a separate round-trip.
        var header = await context.Transactions
            .AsNoTracking()
            .Where(t => t.Id == request.TransactionId)
            .Select(t => new
            {
                t.Id,
                t.TransactionNumber,
                t.CreatedAt,
                t.Notes,
                t.CashierId,
                t.BranchId,
                CashierName = t.Cashier.FirstName + " " + t.Cashier.LastName,
                Company = new ReceiptCompanyDto(
                    t.Tenant.Name,
                    t.Tenant.Tagline,
                    t.Tenant.TaxId,
                    t.Tenant.IsVatRegistered,
                    t.Tenant.FacebookUrl,
                    t.Tenant.InstagramHandle,
                    t.Tenant.GCashNumber),
                Branch = new ReceiptBranchDto(
                    t.Branch.Id,
                    t.Branch.Name,
                    t.Branch.Address,
                    t.Branch.ContactNumber),
                Vehicle = new ReceiptVehicleDto(
                    t.Car.PlateNumber,
                    t.Car.VehicleType.Name,
                    t.Car.Size.Name,
                    t.Car.Make != null ? t.Car.Make.Name : null,
                    t.Car.Model != null ? t.Car.Model.Name : null,
                    t.Car.Color,
                    t.Car.Year),
                Customer = t.Customer != null
                    ? new ReceiptCustomerDto(
                        t.Customer.Id,
                        t.Customer.FirstName + " " + t.Customer.LastName,
                        t.Customer.ContactNumber)
                    : null,
                t.TotalAmount,
                t.DiscountAmount,
                t.TaxAmount,
                t.FinalAmount,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (header is null)
            return null;

        // ── Resolve receipt setting ───────────────────────────────────────────
        // Branch-specific row first (slice 4), then tenant default. Falls back
        // to in-memory defaults for tenants from before slice 2.
        var setting = await context.ReceiptSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.BranchId == header.BranchId, cancellationToken);

        setting ??= await context.ReceiptSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.BranchId == null, cancellationToken);

        setting ??= new ReceiptSetting(string.Empty); // unsaved defaults

        // ── Query 2: service lines ─────────────────────────────────────────────
        var serviceLines = await context.TransactionServices
            .AsNoTracking()
            .Where(ts => ts.TransactionId == request.TransactionId)
            .Select(ts => new
            {
                ts.Service.Name,
                ts.UnitPrice,
                EmployeeNames = ts.EmployeeAssignments
                    .Select(a => a.Employee.FirstName + " " + a.Employee.LastName)
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        // ── Query 3: package lines ─────────────────────────────────────────────
        var packageLines = await context.TransactionPackages
            .AsNoTracking()
            .Where(tp => tp.TransactionId == request.TransactionId)
            .Select(tp => new
            {
                tp.Package.Name,
                tp.UnitPrice,
                EmployeeNames = tp.EmployeeAssignments
                    .Select(a => a.Employee.FirstName + " " + a.Employee.LastName)
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        // ── Query 4: merchandise lines ─────────────────────────────────────────
        var merchandiseLines = await context.TransactionMerchandise
            .AsNoTracking()
            .Where(tm => tm.TransactionId == request.TransactionId)
            .Select(tm => new
            {
                tm.Merchandise.Name,
                tm.UnitPrice,
                tm.Quantity,
            })
            .ToListAsync(cancellationToken);

        // ── Query 5: payments ──────────────────────────────────────────────────
        var payments = await context.Payments
            .AsNoTracking()
            .Where(p => p.TransactionId == request.TransactionId)
            .OrderBy(p => p.PaidAt)
            .Select(p => new ReceiptPaymentDto(
                p.PaymentMethod,
                p.Amount,
                p.ReferenceNumber,
                p.PaidAt))
            .ToListAsync(cancellationToken);

        // ── Assemble line items ────────────────────────────────────────────────
        var lineItems = new List<ReceiptLineItemDto>();

        lineItems.AddRange(serviceLines.Select(s => new ReceiptLineItemDto(
            ReceiptLineType.Service,
            s.Name,
            s.UnitPrice,
            1,
            s.UnitPrice,
            s.EmployeeNames)));

        lineItems.AddRange(packageLines.Select(p => new ReceiptLineItemDto(
            ReceiptLineType.Package,
            p.Name,
            p.UnitPrice,
            1,
            p.UnitPrice,
            p.EmployeeNames)));

        lineItems.AddRange(merchandiseLines.Select(m => new ReceiptLineItemDto(
            ReceiptLineType.Merchandise,
            m.Name,
            m.UnitPrice,
            m.Quantity,
            m.Quantity * m.UnitPrice,
            [])));

        var settingsDto = new ReceiptSettingsDto(
            // Header
            setting.ShowLogo,
            setting.LogoSize,
            setting.LogoPosition,
            setting.ShowBusinessName,
            setting.ShowTagline,
            setting.ShowBranchName,
            setting.ShowBranchAddress,
            setting.ShowBranchContact,
            setting.ShowTIN,
            setting.CustomHeaderText,
            // Body
            setting.ShowServiceDuration,
            setting.ShowEmployeeNames,
            setting.ShowVehicleInfo,
            setting.ShowDiscountBreakdown,
            setting.ShowTaxLine,
            setting.ShowTransactionNumber,
            setting.ShowDateTime,
            setting.ShowCashierName,
            // Customer
            setting.ShowCustomerName,
            setting.ShowCustomerPhone,
            setting.ShowLoyaltyPointsEarned,
            setting.ShowLoyaltyBalance,
            setting.ShowLoyaltyTier,
            // Footer
            setting.ThankYouMessage,
            setting.PromoText,
            setting.ShowSocialMedia,
            setting.ShowGCashQr,
            setting.ShowGCashNumber,
            setting.CustomFooterText,
            // Format
            setting.ReceiptWidth,
            setting.FontSize);

        return new ReceiptDto(
            header.Id,
            header.TransactionNumber,
            header.CreatedAt,
            header.Company,
            header.Branch,
            header.Vehicle,
            header.Customer,
            header.CashierName,
            lineItems,
            header.TotalAmount,
            header.DiscountAmount,
            header.TaxAmount,
            header.FinalAmount,
            payments,
            header.Notes,
            settingsDto);
    }
}
