using System.Net.Http;
using MediatR;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SplashSphere.Application.Features.Transactions.Queries.GetReceipt;

namespace SplashSphere.Application.Features.Transactions.Queries.ExportReceiptPdf;

public sealed class ExportReceiptPdfQueryHandler(
    ISender sender,
    IHttpClientFactory httpClientFactory,
    ILogger<ExportReceiptPdfQueryHandler> logger)
    : IRequestHandler<ExportReceiptPdfQuery, ReceiptPdfResult?>
{
    static ExportReceiptPdfQueryHandler()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<ReceiptPdfResult?> Handle(
        ExportReceiptPdfQuery request,
        CancellationToken cancellationToken)
    {
        var receipt = await sender.Send(new GetReceiptQuery(request.TransactionId), cancellationToken);
        if (receipt is null)
            return null;

        // Prefetch the logo image so QuestPDF's synchronous Compose() can
        // embed it as bytes. We swallow fetch failures — a missing/broken
        // logo URL must not break the receipt PDF, just fall back to the
        // text-only header. The renderer treats `null` as "no logo".
        byte[]? logoBytes = null;
        if (receipt.Settings.ShowLogo && !string.IsNullOrWhiteSpace(receipt.Company.LogoThumbnailUrl))
        {
            try
            {
                using var http = httpClientFactory.CreateClient();
                http.Timeout = TimeSpan.FromSeconds(5);
                logoBytes = await http.GetByteArrayAsync(
                    receipt.Company.LogoThumbnailUrl,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to fetch logo {Url} for receipt {TxNumber}; falling back to text-only header",
                    receipt.Company.LogoThumbnailUrl, receipt.TransactionNumber);
            }
        }

        var document = new ReceiptPdfDocument(receipt, logoBytes);
        var pdfBytes = document.GeneratePdf();

        var fileName = $"receipt_{receipt.TransactionNumber}.pdf";
        return new ReceiptPdfResult(fileName, pdfBytes);
    }
}
