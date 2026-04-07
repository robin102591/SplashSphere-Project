using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Transactions.Queries.ExportReceiptPdf;

public sealed record ExportReceiptPdfQuery(string TransactionId) : IQuery<ReceiptPdfResult?>;

public sealed record ReceiptPdfResult(string FileName, byte[] Content);
