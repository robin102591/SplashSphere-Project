using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Queries.ExportPayslipPdf;

public sealed record ExportPayslipPdfQuery(string EntryId) : IQuery<PayslipPdfResult?>;

public sealed record PayslipPdfResult(string FileName, byte[] Content);
