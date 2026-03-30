using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SplashSphere.Application.Features.Payroll.Queries.GetPayslip;

namespace SplashSphere.Application.Features.Payroll.Queries.ExportPayslipPdf;

public sealed class ExportPayslipPdfQueryHandler(ISender sender)
    : IRequestHandler<ExportPayslipPdfQuery, PayslipPdfResult?>
{
    static ExportPayslipPdfQueryHandler()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<PayslipPdfResult?> Handle(
        ExportPayslipPdfQuery request,
        CancellationToken cancellationToken)
    {
        var payslip = await sender.Send(new GetPayslipQuery(request.EntryId), cancellationToken);
        if (payslip is null)
            return null;

        var document = new PayslipPdfDocument(payslip);
        var pdfBytes = document.GeneratePdf();

        var safeName = payslip.EmployeeName.Replace(" ", "_").ToLowerInvariant();
        var fileName = $"payslip_{safeName}_{payslip.PeriodStart:yyyyMMdd}.pdf";

        return new PayslipPdfResult(fileName, pdfBytes);
    }
}
