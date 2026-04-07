using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SplashSphere.Application.Features.Transactions.Queries.GetReceipt;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Transactions.Queries.ExportReceiptPdf;

public sealed class ReceiptPdfDocument(ReceiptDto data) : IDocument
{
    // 80mm thermal receipt width ≈ 226 points (80mm × 2.83 pts/mm)
    private const float ReceiptWidth = 226f;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(ReceiptWidth, 1000); // tall page, content determines height
            page.MarginHorizontal(8);
            page.MarginVertical(10);
            page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Courier New"));

            page.Content().Column(col =>
            {
                // ── Header ────────────────────────────────────────────────────
                col.Item().AlignCenter().Text("SplashSphere").Bold().FontSize(14);
                col.Item().AlignCenter().Text(data.Branch.Name).Bold().FontSize(10);
                col.Item().AlignCenter().Text(data.Branch.Address).FontSize(7);
                col.Item().AlignCenter().Text(data.Branch.ContactNumber).FontSize(7);

                DashedLine(col);

                // Transaction number + date
                col.Item().AlignCenter().Text(data.TransactionNumber).Bold().FontSize(10);
                col.Item().AlignCenter().Text(data.IssuedAt.ToString("MMM d, yyyy h:mm tt")).FontSize(7);

                DashedLine(col);

                // ── Vehicle / Customer info ───────────────────────────────────
                col.Item().Text(t =>
                {
                    t.Span("Plate: ").FontSize(8);
                    t.Span(data.Vehicle.PlateNumber).Bold().FontSize(9);
                });
                col.Item().Text($"Vehicle: {data.Vehicle.VehicleTypeName} · {data.Vehicle.SizeName}").FontSize(8);
                if (data.Customer is not null)
                    col.Item().Text($"Customer: {data.Customer.Name}").FontSize(8);
                col.Item().Text($"Cashier: {data.CashierName}").FontSize(8);

                DashedLine(col);

                // ── Line items ────────────────────────────────────────────────
                foreach (var item in data.LineItems)
                {
                    var label = item.Quantity > 1
                        ? $"{item.Name} x{item.Quantity}"
                        : item.Name;

                    col.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text(label).FontSize(8);
                        row.RelativeItem(1).AlignRight().Text(FormatPeso(item.LineTotal)).FontSize(8);
                    });
                }

                DashedLine(col);

                // ── Totals ────────────────────────────────────────────────────
                TotalRow(col, "Subtotal", data.SubTotal);

                if (data.DiscountAmount > 0)
                    TotalRow(col, "Discount", -data.DiscountAmount);

                if (data.TaxAmount > 0)
                    TotalRow(col, "Tax", data.TaxAmount);

                col.Item().PaddingTop(2).LineHorizontal(0.5f);
                col.Item().PaddingTop(2).Row(row =>
                {
                    row.RelativeItem(3).Text("TOTAL").Bold().FontSize(10);
                    row.RelativeItem(1).AlignRight().Text(FormatPeso(data.TotalAmount)).Bold().FontSize(10);
                });

                // ── Payments ──────────────────────────────────────────────────
                if (data.Payments.Count > 0)
                {
                    DashedLine(col);

                    foreach (var payment in data.Payments)
                    {
                        TotalRow(col, PaymentLabel(payment.Method), payment.Amount);
                    }

                    var totalPaid = data.Payments.Sum(p => p.Amount);
                    var change = totalPaid - data.TotalAmount;
                    if (change > 0.01m)
                    {
                        col.Item().PaddingTop(2).Row(row =>
                        {
                            row.RelativeItem(3).Text("CHANGE").Bold().FontSize(9);
                            row.RelativeItem(1).AlignRight().Text(FormatPeso(change)).Bold().FontSize(9);
                        });
                    }
                }

                DashedLine(col);

                // ── Footer ────────────────────────────────────────────────────
                col.Item().PaddingTop(4).AlignCenter().Text("Thank you for choosing").FontSize(8);
                col.Item().AlignCenter().Text("SplashSphere!").Bold().FontSize(8);

                if (data.Notes is not null)
                {
                    col.Item().PaddingTop(4).Text($"Notes: {data.Notes}").FontSize(7).Italic();
                }
            });
        });
    }

    private static void DashedLine(ColumnDescriptor col)
    {
        col.Item().PaddingVertical(4).AlignCenter()
            .Text("- - - - - - - - - - - - - - - - - - - -")
            .FontSize(6);
    }

    private static void TotalRow(ColumnDescriptor col, string label, decimal amount)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem(3).Text(label).FontSize(8);
            row.RelativeItem(1).AlignRight().Text(FormatPeso(amount)).FontSize(8);
        });
    }

    private static string FormatPeso(decimal amount)
    {
        return $"P{Math.Abs(amount):N2}";
    }

    private static string PaymentLabel(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "Cash",
        PaymentMethod.GCash => "GCash / Maya",
        PaymentMethod.CreditCard => "Credit Card",
        PaymentMethod.DebitCard => "Debit Card",
        PaymentMethod.BankTransfer => "Bank Transfer",
        _ => "Payment",
    };
}
