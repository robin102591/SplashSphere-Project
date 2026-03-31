using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Billing.Queries.ExportInvoicePdf;

public sealed record ExportInvoicePdfQuery(string BillingRecordId) : IQuery<InvoicePdfResult?>;

public sealed record InvoicePdfResult(string FileName, byte[] Content);
