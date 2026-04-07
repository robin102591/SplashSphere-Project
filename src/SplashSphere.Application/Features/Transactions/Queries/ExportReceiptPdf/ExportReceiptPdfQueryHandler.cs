using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SplashSphere.Application.Features.Transactions.Queries.GetReceipt;

namespace SplashSphere.Application.Features.Transactions.Queries.ExportReceiptPdf;

public sealed class ExportReceiptPdfQueryHandler(ISender sender)
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

        var document = new ReceiptPdfDocument(receipt);
        var pdfBytes = document.GeneratePdf();

        var fileName = $"receipt_{receipt.TransactionNumber}.pdf";

        return new ReceiptPdfResult(fileName, pdfBytes);
    }
}
