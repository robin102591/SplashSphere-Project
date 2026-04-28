using System.Globalization;
using System.Text;
using SplashSphere.Application.Features.Transactions.Queries.GetReceipt;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Transactions.Services;

/// <summary>
/// Renders a <see cref="ReceiptDto"/> as an HTML email body. Honors the same
/// <see cref="ReceiptSettingsDto"/> toggles as <c>ReceiptPdfDocument</c> so
/// the email and the printed receipt show consistent content.
/// </summary>
/// <remarks>
/// All styles are inline. Email clients (Gmail, Outlook, Apple Mail) strip
/// or ignore <c>&lt;link&gt;</c> stylesheets and most <c>&lt;style&gt;</c>
/// blocks, so we cannot rely on external CSS. The width is constrained to
/// 600px (a near-universal email-template baseline) with mobile fluidity
/// via percentage widths inside the wrapper.
/// </remarks>
public static class ReceiptHtmlRenderer
{
    public static string Render(ReceiptDto data)
    {
        var s = data.Settings;
        var sb = new StringBuilder(8 * 1024);

        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Receipt</title></head>");
        sb.Append("<body style=\"margin:0;padding:24px 12px;background:#f5f5f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;color:#111;\">");
        sb.Append("<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;max-width:600px;margin:0 auto;background:#ffffff;border:1px solid #e5e5e5;border-radius:8px;overflow:hidden;\">");

        // ── Header ────────────────────────────────────────────────────────────
        sb.Append("<tr><td style=\"padding:24px 24px 16px;text-align:");
        sb.Append(s.LogoPosition == LogoPosition.Left ? "left" : "center");
        sb.Append(";border-bottom:1px solid #e5e5e5;\">");

        if (s.ShowLogo && !string.IsNullOrWhiteSpace(data.Company.LogoThumbnailUrl))
        {
            var logoSize = s.LogoSize switch
            {
                LogoSize.Small => 48,
                LogoSize.Large => 96,
                _              => 64,
            };
            sb.Append("<img src=\"").Append(HtmlEscape(data.Company.LogoThumbnailUrl!))
              .Append("\" alt=\"").Append(HtmlEscape(data.Company.BusinessName)).Append("\" ")
              .Append("style=\"width:").Append(logoSize).Append("px;height:").Append(logoSize)
              .Append("px;object-fit:contain;display:block;")
              .Append(s.LogoPosition == LogoPosition.Center ? "margin:0 auto 12px;" : "margin:0 0 12px;")
              .Append("\">");
        }

        if (s.ShowBusinessName)
            sb.Append("<div style=\"font-size:20px;font-weight:700;\">").Append(HtmlEscape(data.Company.BusinessName)).Append("</div>");

        if (s.ShowTagline && !string.IsNullOrWhiteSpace(data.Company.Tagline))
            sb.Append("<div style=\"font-size:13px;color:#666;font-style:italic;margin-top:2px;\">").Append(HtmlEscape(data.Company.Tagline!)).Append("</div>");

        if (s.ShowBranchName)
            sb.Append("<div style=\"font-size:14px;font-weight:600;margin-top:8px;\">").Append(HtmlEscape(data.Branch.Name)).Append("</div>");

        if (s.ShowBranchAddress)
            sb.Append("<div style=\"font-size:13px;color:#444;\">").Append(HtmlEscape(data.Branch.Address)).Append("</div>");

        if (s.ShowBranchContact)
            sb.Append("<div style=\"font-size:13px;color:#444;\">Tel: ").Append(HtmlEscape(data.Branch.ContactNumber)).Append("</div>");

        if (s.ShowTIN && !string.IsNullOrWhiteSpace(data.Company.TaxId))
            sb.Append("<div style=\"font-size:13px;color:#444;\">TIN: ").Append(HtmlEscape(data.Company.TaxId!)).Append("</div>");

        if (!string.IsNullOrWhiteSpace(s.CustomHeaderText))
            sb.Append("<div style=\"font-size:12px;color:#666;margin-top:8px;\">").Append(HtmlEscape(s.CustomHeaderText!)).Append("</div>");

        sb.Append("</td></tr>");

        // ── Transaction meta ──────────────────────────────────────────────────
        if (s.ShowTransactionNumber || s.ShowDateTime || s.ShowCashierName)
        {
            sb.Append("<tr><td style=\"padding:16px 24px;border-bottom:1px solid #e5e5e5;font-size:13px;\">");
            if (s.ShowTransactionNumber)
                sb.Append("<div><strong>Transaction:</strong> ").Append(HtmlEscape(data.TransactionNumber)).Append("</div>");
            if (s.ShowDateTime)
                sb.Append("<div><strong>Date:</strong> ").Append(data.IssuedAt.ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture)).Append("</div>");
            if (s.ShowCashierName)
                sb.Append("<div><strong>Cashier:</strong> ").Append(HtmlEscape(data.CashierName)).Append("</div>");
            sb.Append("</td></tr>");
        }

        // ── Vehicle ───────────────────────────────────────────────────────────
        if (s.ShowVehicleInfo)
        {
            sb.Append("<tr><td style=\"padding:16px 24px;border-bottom:1px solid #e5e5e5;font-size:13px;\">");
            sb.Append("<div><strong>Plate:</strong> ").Append(HtmlEscape(data.Vehicle.PlateNumber)).Append("</div>");
            sb.Append("<div><strong>Vehicle:</strong> ").Append(HtmlEscape(data.Vehicle.VehicleTypeName))
              .Append(" · ").Append(HtmlEscape(data.Vehicle.SizeName)).Append("</div>");
            sb.Append("</td></tr>");
        }

        // ── Customer ──────────────────────────────────────────────────────────
        if (data.Customer is not null && (s.ShowCustomerName || s.ShowCustomerPhone))
        {
            sb.Append("<tr><td style=\"padding:16px 24px;border-bottom:1px solid #e5e5e5;font-size:13px;\">");
            if (s.ShowCustomerName)
                sb.Append("<div><strong>Customer:</strong> ").Append(HtmlEscape(data.Customer.Name)).Append("</div>");
            if (s.ShowCustomerPhone && !string.IsNullOrWhiteSpace(data.Customer.ContactNumber))
                sb.Append("<div><strong>Phone:</strong> ").Append(HtmlEscape(data.Customer.ContactNumber)).Append("</div>");
            sb.Append("</td></tr>");
        }

        // ── Line items ────────────────────────────────────────────────────────
        sb.Append("<tr><td style=\"padding:16px 24px;border-bottom:1px solid #e5e5e5;\">");
        sb.Append("<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:13px;\">");
        foreach (var item in data.LineItems)
        {
            var label = item.Quantity > 1 ? $"{item.Name} × {item.Quantity}" : item.Name;
            sb.Append("<tr>")
              .Append("<td style=\"padding:4px 0;\">").Append(HtmlEscape(label)).Append("</td>")
              .Append("<td style=\"padding:4px 0;text-align:right;font-variant-numeric:tabular-nums;\">")
              .Append(FormatPeso(item.LineTotal)).Append("</td></tr>");

            if (s.ShowEmployeeNames && item.AssignedEmployees.Count > 0)
                sb.Append("<tr><td colspan=\"2\" style=\"padding:0 0 4px 12px;font-size:12px;color:#666;font-style:italic;\">")
                  .Append(HtmlEscape(string.Join(" & ", item.AssignedEmployees))).Append("</td></tr>");
        }
        sb.Append("</table></td></tr>");

        // ── Totals ────────────────────────────────────────────────────────────
        sb.Append("<tr><td style=\"padding:16px 24px;border-bottom:1px solid #e5e5e5;\">");
        sb.Append("<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:13px;\">");
        sb.Append("<tr><td style=\"padding:2px 0;\">Subtotal</td><td style=\"padding:2px 0;text-align:right;font-variant-numeric:tabular-nums;\">")
          .Append(FormatPeso(data.SubTotal)).Append("</td></tr>");

        if (data.DiscountAmount > 0 && s.ShowDiscountBreakdown)
            sb.Append("<tr><td style=\"padding:2px 0;\">Discount</td><td style=\"padding:2px 0;text-align:right;font-variant-numeric:tabular-nums;color:#c0392b;\">-")
              .Append(FormatPeso(data.DiscountAmount)).Append("</td></tr>");

        if (data.TaxAmount > 0 && s.ShowTaxLine)
            sb.Append("<tr><td style=\"padding:2px 0;\">VAT</td><td style=\"padding:2px 0;text-align:right;font-variant-numeric:tabular-nums;\">")
              .Append(FormatPeso(data.TaxAmount)).Append("</td></tr>");

        sb.Append("<tr><td style=\"padding:8px 0 2px;border-top:2px solid #111;font-weight:700;font-size:15px;\">TOTAL</td>")
          .Append("<td style=\"padding:8px 0 2px;border-top:2px solid #111;text-align:right;font-weight:700;font-size:15px;font-variant-numeric:tabular-nums;\">")
          .Append(FormatPeso(data.TotalAmount)).Append("</td></tr>");

        // Payments
        foreach (var p in data.Payments)
        {
            sb.Append("<tr><td style=\"padding:2px 0;color:#666;\">").Append(HtmlEscape(PaymentLabel(p.Method))).Append("</td>")
              .Append("<td style=\"padding:2px 0;text-align:right;color:#666;font-variant-numeric:tabular-nums;\">")
              .Append(FormatPeso(p.Amount)).Append("</td></tr>");
        }
        var totalPaid = data.Payments.Sum(p => p.Amount);
        var change = totalPaid - data.TotalAmount;
        if (change > 0.01m)
        {
            sb.Append("<tr><td style=\"padding:2px 0;font-weight:600;\">Change</td>")
              .Append("<td style=\"padding:2px 0;text-align:right;font-weight:600;font-variant-numeric:tabular-nums;\">")
              .Append(FormatPeso(change)).Append("</td></tr>");
        }
        sb.Append("</table></td></tr>");

        // ── Footer ────────────────────────────────────────────────────────────
        sb.Append("<tr><td style=\"padding:24px;text-align:center;font-size:13px;\">");
        sb.Append("<div style=\"font-weight:600;font-size:15px;\">").Append(HtmlEscape(s.ThankYouMessage)).Append("</div>");

        if (!string.IsNullOrWhiteSpace(s.PromoText))
            sb.Append("<div style=\"margin-top:12px;color:#444;\">").Append(HtmlEscape(s.PromoText!)).Append("</div>");

        if (s.ShowSocialMedia)
        {
            if (!string.IsNullOrWhiteSpace(data.Company.FacebookUrl))
                sb.Append("<div style=\"margin-top:12px;\"><a href=\"").Append(HtmlEscape(data.Company.FacebookUrl!))
                  .Append("\" style=\"color:#2563eb;text-decoration:none;\">Facebook</a></div>");
            if (!string.IsNullOrWhiteSpace(data.Company.InstagramHandle))
                sb.Append("<div style=\"margin-top:4px;color:#444;\">Instagram: ").Append(HtmlEscape(data.Company.InstagramHandle!)).Append("</div>");
        }

        if (s.ShowGCashNumber && !string.IsNullOrWhiteSpace(data.Company.GCashNumber))
            sb.Append("<div style=\"margin-top:8px;color:#444;\">GCash: ").Append(HtmlEscape(data.Company.GCashNumber!)).Append("</div>");

        if (!string.IsNullOrWhiteSpace(s.CustomFooterText))
            sb.Append("<div style=\"margin-top:16px;font-size:11px;color:#888;\">").Append(HtmlEscape(s.CustomFooterText!)).Append("</div>");

        sb.Append("</td></tr>");

        sb.Append("</table></body></html>");
        return sb.ToString();
    }

    private static string FormatPeso(decimal amount)
        => $"₱{amount.ToString("N2", new CultureInfo("en-PH"))}";

    private static string PaymentLabel(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash         => "Cash",
        PaymentMethod.GCash        => "GCash / Maya",
        PaymentMethod.CreditCard   => "Credit Card",
        PaymentMethod.DebitCard    => "Debit Card",
        PaymentMethod.BankTransfer => "Bank Transfer",
        _                          => "Payment",
    };

    /// <summary>
    /// Minimal HTML attribute / text escaping. We only emit user-controlled
    /// data inside text nodes and double-quoted attributes, so escaping &amp;,
    /// &lt;, &gt;, and " covers every site. Avoids pulling in System.Web for
    /// a single <c>HttpUtility.HtmlEncode</c> call.
    /// </summary>
    private static string HtmlEscape(string raw) => raw
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;");
}
