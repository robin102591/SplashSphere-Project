using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Subscription;

namespace SplashSphere.Application.Features.Billing.Queries.ExportInvoicePdf;

public sealed class ExportInvoicePdfQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ExportInvoicePdfQuery, InvoicePdfResult?>
{
    static ExportInvoicePdfQueryHandler()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<InvoicePdfResult?> Handle(
        ExportInvoicePdfQuery request,
        CancellationToken cancellationToken)
    {
        var record = await db.BillingRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(b => b.Tenant)
            .Include(b => b.Subscription)
            .FirstOrDefaultAsync(b => b.Id == request.BillingRecordId, cancellationToken);

        if (record is null) return null;

        var plan = PlanCatalog.GetPlan(record.Subscription.PlanTier);

        var data = new InvoiceData(
            record.InvoiceNumber ?? $"INV-{record.Id[..8].ToUpperInvariant()}",
            record.Tenant.Name,
            record.Tenant.Email,
            record.Tenant.Address,
            plan.Name,
            record.Amount,
            record.Currency,
            record.Status.ToString(),
            record.BillingDate,
            record.DueDate,
            record.PaidDate,
            record.PaymentMethod);

        var document = new InvoicePdfDocument(data);
        var pdfBytes = document.GeneratePdf();

        var invoiceNum = record.InvoiceNumber?.Replace("/", "-") ?? record.Id[..8];
        var fileName = $"invoice_{invoiceNum}.pdf";

        return new InvoicePdfResult(fileName, pdfBytes);
    }
}
