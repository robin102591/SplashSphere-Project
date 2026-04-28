using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SplashSphere.Application.Features.Transactions.Queries.GetReceipt;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Transactions.Queries.ExportReceiptPdf;

/// <summary>
/// Renders a transaction receipt as a thermal-style PDF. Honors the
/// per-tenant <see cref="Domain.Entities.ReceiptSetting"/> toggles propagated
/// through <see cref="ReceiptDto.Settings"/> — each <c>Show*</c> flag gates
/// the corresponding section. Fields tied to data not yet on the wire
/// (loyalty info, service durations) are no-ops in the renderer; the toggles
/// stay in the form so they take effect when the data lands.
/// </summary>
/// <param name="data">Composed receipt data for this transaction.</param>
/// <param name="logoBytes">
/// Optional logo image bytes. When non-null AND <see cref="ReceiptSettingsDto.ShowLogo"/>
/// is true, QuestPDF embeds the image at the top of the header. The handler
/// (not the document) is responsible for fetching from R2 — keeping I/O out
/// of the synchronous Compose() pipeline.
/// </param>
public sealed class ReceiptPdfDocument(ReceiptDto data, byte[]? logoBytes = null) : IDocument
{
    // 1mm = 2.83465 points. We render at the configured width minus margins.
    private const float Mm58Width = 164f;   // 58mm  → ~164pt
    private const float Mm80Width = 226f;   // 80mm  → ~226pt

    public void Compose(IDocumentContainer container)
    {
        var s = data.Settings;

        var pageWidth = s.ReceiptWidth == ReceiptWidth.Mm80 ? Mm80Width : Mm58Width;
        var bodyFont  = BodyFontSize(s.FontSize);
        var smallFont = bodyFont - 1;
        var titleFont = bodyFont + 4;
        var totalFont = bodyFont + 2;

        container.Page(page =>
        {
            page.Size(pageWidth, 1000); // tall page; content determines height
            page.MarginHorizontal(8);
            page.MarginVertical(10);
            page.DefaultTextStyle(x => x.FontSize(bodyFont).FontFamily("Courier New"));

            page.Content().Column(col =>
            {
                // ── Header ────────────────────────────────────────────────────
                ComposeHeader(col, s, data, titleFont, smallFont);

                // ── Transaction meta (tx number, date, cashier) ───────────────
                if (s.ShowTransactionNumber || s.ShowDateTime || s.ShowCashierName)
                {
                    DashedLine(col);
                    if (s.ShowTransactionNumber)
                        col.Item().AlignCenter().Text(data.TransactionNumber).Bold().FontSize(totalFont);
                    if (s.ShowDateTime)
                        col.Item().AlignCenter()
                            .Text(data.IssuedAt.ToString("MMM d, yyyy h:mm tt"))
                            .FontSize(smallFont);
                    if (s.ShowCashierName)
                        col.Item().Text($"Cashier: {data.CashierName}").FontSize(bodyFont);
                }

                // ── Vehicle info ──────────────────────────────────────────────
                if (s.ShowVehicleInfo)
                {
                    DashedLine(col);
                    col.Item().Text(t =>
                    {
                        t.Span("Plate: ").FontSize(bodyFont);
                        t.Span(data.Vehicle.PlateNumber).Bold().FontSize(bodyFont + 1);
                    });
                    col.Item().Text($"Vehicle: {data.Vehicle.VehicleTypeName} · {data.Vehicle.SizeName}")
                        .FontSize(bodyFont);
                }

                // ── Customer ──────────────────────────────────────────────────
                if (data.Customer is not null && (s.ShowCustomerName || s.ShowCustomerPhone))
                {
                    DashedLine(col);
                    if (s.ShowCustomerName)
                        col.Item().Text($"Customer: {data.Customer.Name}").FontSize(bodyFont);
                    if (s.ShowCustomerPhone && !string.IsNullOrWhiteSpace(data.Customer.ContactNumber))
                        col.Item().Text($"Phone: {data.Customer.ContactNumber}").FontSize(bodyFont);
                }

                // ── Line items ────────────────────────────────────────────────
                DashedLine(col);
                foreach (var item in data.LineItems)
                {
                    var label = item.Quantity > 1
                        ? $"{item.Name} x{item.Quantity}"
                        : item.Name;

                    col.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text(label).FontSize(bodyFont);
                        row.RelativeItem(1).AlignRight().Text(FormatPeso(item.LineTotal)).FontSize(bodyFont);
                    });

                    // Sub-line: employees who performed the service.
                    // Service duration toggle is reserved for a future Service.Duration field.
                    if (s.ShowEmployeeNames && item.AssignedEmployees.Count > 0)
                    {
                        col.Item().PaddingLeft(8)
                            .Text(string.Join(" & ", item.AssignedEmployees))
                            .FontSize(smallFont).Italic();
                    }
                }

                // ── Totals ────────────────────────────────────────────────────
                DashedLine(col);
                TotalRow(col, "Subtotal", data.SubTotal, bodyFont);

                if (data.DiscountAmount > 0 && s.ShowDiscountBreakdown)
                    TotalRow(col, "Discount", -data.DiscountAmount, bodyFont);

                if (data.TaxAmount > 0 && s.ShowTaxLine)
                    TotalRow(col, "VAT", data.TaxAmount, bodyFont);

                col.Item().PaddingTop(2).LineHorizontal(0.5f);
                col.Item().PaddingTop(2).Row(row =>
                {
                    row.RelativeItem(3).Text("TOTAL").Bold().FontSize(totalFont);
                    row.RelativeItem(1).AlignRight().Text(FormatPeso(data.TotalAmount)).Bold().FontSize(totalFont);
                });

                // ── Payments ──────────────────────────────────────────────────
                if (data.Payments.Count > 0)
                {
                    DashedLine(col);

                    foreach (var payment in data.Payments)
                        TotalRow(col, PaymentLabel(payment.Method), payment.Amount, bodyFont);

                    var totalPaid = data.Payments.Sum(p => p.Amount);
                    var change = totalPaid - data.TotalAmount;
                    if (change > 0.01m)
                    {
                        col.Item().PaddingTop(2).Row(row =>
                        {
                            row.RelativeItem(3).Text("CHANGE").Bold().FontSize(bodyFont + 1);
                            row.RelativeItem(1).AlignRight().Text(FormatPeso(change)).Bold().FontSize(bodyFont + 1);
                        });
                    }
                }

                // ── Footer ────────────────────────────────────────────────────
                ComposeFooter(col, s, data, bodyFont, smallFont);

                if (data.Notes is not null)
                    col.Item().PaddingTop(4).Text($"Notes: {data.Notes}").FontSize(smallFont).Italic();
            });
        });
    }

    private void ComposeHeader(
        ColumnDescriptor col,
        ReceiptSettingsDto s,
        ReceiptDto data,
        int titleFont,
        int smallFont)
    {
        // Logo: when ShowLogo is on AND we have prefetched bytes, embed the
        // image. When the toggle is on but bytes are null (no logo uploaded
        // or fetch failed), we render nothing — the text-only header below
        // still gives the receipt identity.
        if (s.ShowLogo && logoBytes is not null)
        {
            var px = s.LogoSize switch
            {
                LogoSize.Small  => 32,
                LogoSize.Large  => 64,
                _               => 48,
            };

            var logoBlock = s.LogoPosition == LogoPosition.Left
                ? col.Item().AlignLeft()
                : col.Item().AlignCenter();

            logoBlock.Height(px).Image(logoBytes).FitArea();
        }

        // Center vs left alignment of the textual header is governed by the
        // logo position to keep visual balance.
        var alignedHeader = s.LogoPosition == LogoPosition.Left
            ? col.Item().AlignLeft()
            : col.Item().AlignCenter();

        if (s.ShowBusinessName)
            alignedHeader.Text(data.Company.BusinessName).Bold().FontSize(titleFont);

        if (s.ShowTagline && !string.IsNullOrWhiteSpace(data.Company.Tagline))
        {
            (s.LogoPosition == LogoPosition.Left ? col.Item().AlignLeft() : col.Item().AlignCenter())
                .Text(data.Company.Tagline!).Italic().FontSize(smallFont);
        }

        if (s.ShowBranchName)
            (s.LogoPosition == LogoPosition.Left ? col.Item().AlignLeft() : col.Item().AlignCenter())
                .Text(data.Branch.Name).Bold().FontSize(smallFont + 2);

        if (s.ShowBranchAddress)
            (s.LogoPosition == LogoPosition.Left ? col.Item().AlignLeft() : col.Item().AlignCenter())
                .Text(data.Branch.Address).FontSize(smallFont);

        if (s.ShowBranchContact)
            (s.LogoPosition == LogoPosition.Left ? col.Item().AlignLeft() : col.Item().AlignCenter())
                .Text(data.Branch.ContactNumber).FontSize(smallFont);

        if (s.ShowTIN && !string.IsNullOrWhiteSpace(data.Company.TaxId))
            (s.LogoPosition == LogoPosition.Left ? col.Item().AlignLeft() : col.Item().AlignCenter())
                .Text($"TIN: {data.Company.TaxId}").FontSize(smallFont);

        if (!string.IsNullOrWhiteSpace(s.CustomHeaderText))
            (s.LogoPosition == LogoPosition.Left ? col.Item().AlignLeft() : col.Item().AlignCenter())
                .PaddingTop(1).Text(s.CustomHeaderText!).FontSize(smallFont);
    }

    private static void ComposeFooter(
        ColumnDescriptor col,
        ReceiptSettingsDto s,
        ReceiptDto data,
        int bodyFont,
        int smallFont)
    {
        DashedLine(col);

        col.Item().PaddingTop(2).AlignCenter().Text(s.ThankYouMessage).Bold().FontSize(bodyFont);

        if (!string.IsNullOrWhiteSpace(s.PromoText))
            col.Item().PaddingTop(2).AlignCenter().Text(s.PromoText!).FontSize(smallFont);

        if (s.ShowSocialMedia)
        {
            if (!string.IsNullOrWhiteSpace(data.Company.FacebookUrl))
            {
                var path = data.Company.FacebookUrl!
                    .Replace("https://www.facebook.com", "")
                    .Replace("https://facebook.com", "")
                    .Replace("http://www.facebook.com", "")
                    .Replace("http://facebook.com", "");
                col.Item().PaddingTop(2).AlignCenter().Text($"FB: {path}").FontSize(smallFont);
            }
            if (!string.IsNullOrWhiteSpace(data.Company.InstagramHandle))
                col.Item().AlignCenter().Text($"IG: {data.Company.InstagramHandle}").FontSize(smallFont);
        }

        if (s.ShowGCashNumber && !string.IsNullOrWhiteSpace(data.Company.GCashNumber))
            col.Item().PaddingTop(2).AlignCenter().Text($"GCash: {data.Company.GCashNumber}").FontSize(smallFont);

        // GCash QR image render is reserved for slice 3 (image upload).

        if (!string.IsNullOrWhiteSpace(s.CustomFooterText))
            col.Item().PaddingTop(2).AlignCenter().Text(s.CustomFooterText!).FontSize(smallFont);
    }

    private static int BodyFontSize(ReceiptFontSize size) => size switch
    {
        ReceiptFontSize.Small => 7,
        ReceiptFontSize.Large => 10,
        _                     => 8,
    };

    private static void DashedLine(ColumnDescriptor col)
    {
        col.Item().PaddingVertical(4).AlignCenter()
            .Text("- - - - - - - - - - - - - - - - - - - -")
            .FontSize(6);
    }

    private static void TotalRow(ColumnDescriptor col, string label, decimal amount, int bodyFont)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem(3).Text(label).FontSize(bodyFont);
            row.RelativeItem(1).AlignRight().Text(FormatPeso(amount)).FontSize(bodyFont);
        });
    }

    private static string FormatPeso(decimal amount) => $"P{Math.Abs(amount):N2}";

    private static string PaymentLabel(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash         => "Cash",
        PaymentMethod.GCash        => "GCash / Maya",
        PaymentMethod.CreditCard   => "Credit Card",
        PaymentMethod.DebitCard    => "Debit Card",
        PaymentMethod.BankTransfer => "Bank Transfer",
        _                          => "Payment",
    };
}
