using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SplashSphere.Application.Features.Billing.Queries.ExportInvoicePdf;

public sealed record InvoiceData(
    string InvoiceNumber,
    string TenantName,
    string TenantEmail,
    string TenantAddress,
    string PlanName,
    decimal Amount,
    string Currency,
    string Status,
    DateTime BillingDate,
    DateTime? DueDate,
    DateTime? PaidDate,
    string? PaymentMethod);

public sealed class InvoicePdfDocument(InvoiceData data) : IDocument
{
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.MarginHorizontal(50);
            page.MarginVertical(40);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().Text("SplashSphere").Bold().FontSize(18).FontColor(Colors.Blue.Medium);
                        left.Item().Text("Car Wash Management Platform").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                    row.RelativeItem().AlignRight().Column(right =>
                    {
                        right.Item().Text("INVOICE").Bold().FontSize(22).FontColor(Colors.Grey.Darken2);
                        right.Item().Text(data.InvoiceNumber).FontSize(10).FontColor(Colors.Grey.Medium);
                    });
                });

                col.Item().PaddingTop(15).LineHorizontal(1f).LineColor(Colors.Grey.Lighten2);
            });

            page.Content().PaddingTop(20).Column(col =>
            {
                // Bill To + Invoice Details
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().Text("Bill To:").Bold().FontSize(9).FontColor(Colors.Grey.Medium);
                        left.Item().PaddingTop(4).Text(data.TenantName).Bold();
                        left.Item().Text(data.TenantEmail).FontSize(9);
                        if (!string.IsNullOrEmpty(data.TenantAddress))
                            left.Item().Text(data.TenantAddress).FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                    row.RelativeItem().AlignRight().Column(right =>
                    {
                        right.Item().Text("Invoice Details:").Bold().FontSize(9).FontColor(Colors.Grey.Medium);
                        right.Item().PaddingTop(4).Text($"Date: {data.BillingDate:MMM d, yyyy}");
                        if (data.DueDate.HasValue)
                            right.Item().Text($"Due: {data.DueDate.Value:MMM d, yyyy}");
                        right.Item().Text($"Status: {data.Status}").Bold();
                        if (data.PaidDate.HasValue)
                            right.Item().Text($"Paid: {data.PaidDate.Value:MMM d, yyyy}");
                    });
                });

                // Line Items Table
                col.Item().PaddingTop(30).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(4);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Description").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignCenter().Text("Qty").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text("Price").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text("Amount").Bold().FontSize(9);
                    });

                    // Row
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                        .Text($"{data.PlanName} — Monthly Subscription");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                        .AlignCenter().Text("1");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                        .AlignRight().Text(Peso(data.Amount));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                        .AlignRight().Text(Peso(data.Amount)).Bold();
                });

                // Total
                col.Item().PaddingTop(10).AlignRight().Row(row =>
                {
                    row.ConstantItem(100).AlignRight().Text("Total:").Bold();
                    row.ConstantItem(120).AlignRight().Text(Peso(data.Amount)).Bold().FontSize(14);
                });

                if (data.PaymentMethod is not null)
                {
                    col.Item().PaddingTop(6).AlignRight().Text($"Paid via {data.PaymentMethod}")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                }

                // Notes
                col.Item().PaddingTop(40).Text("Notes:").Bold().FontSize(9).FontColor(Colors.Grey.Medium);
                col.Item().PaddingTop(4).Text("Thank you for subscribing to SplashSphere. For questions, contact support@splashsphere.ph.")
                    .FontSize(9).FontColor(Colors.Grey.Medium);
            });

            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("SplashSphere — ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.Span($"Generated {DateTime.UtcNow:MMM d, yyyy}").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private static string Peso(decimal amount) => $"₱{amount:N2}";
}
