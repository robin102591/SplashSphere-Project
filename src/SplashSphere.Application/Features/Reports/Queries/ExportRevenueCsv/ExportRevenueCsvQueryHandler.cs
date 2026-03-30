using System.Globalization;
using System.Text;
using MediatR;
using SplashSphere.Application.Features.Reports.Queries.GetRevenueReport;

namespace SplashSphere.Application.Features.Reports.Queries.ExportRevenueCsv;

public sealed class ExportRevenueCsvQueryHandler(ISender sender)
    : IRequestHandler<ExportRevenueCsvQuery, ReportCsvResult>
{
    public async Task<ReportCsvResult> Handle(
        ExportRevenueCsvQuery request,
        CancellationToken cancellationToken)
    {
        var report = await sender.Send(
            new GetRevenueReportQuery(request.From, request.To, request.BranchId), cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Date,Revenue,Discount,Tax,Transactions");

        foreach (var d in report.DailyBreakdown)
        {
            sb.AppendLine(string.Join(",",
                d.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                d.Revenue.ToString("F2", CultureInfo.InvariantCulture),
                d.Discount.ToString("F2", CultureInfo.InvariantCulture),
                d.Tax.ToString("F2", CultureInfo.InvariantCulture),
                d.TransactionCount.ToString(CultureInfo.InvariantCulture)));
        }

        sb.AppendLine();
        sb.AppendLine("Payment Method,Amount,Count");
        foreach (var p in report.ByPaymentMethod)
        {
            sb.AppendLine(string.Join(",",
                Escape(p.PaymentMethod),
                p.Amount.ToString("F2", CultureInfo.InvariantCulture),
                p.PaymentCount.ToString(CultureInfo.InvariantCulture)));
        }

        var fileName = $"revenue_{request.From:yyyyMMdd}_{request.To:yyyyMMdd}.csv";
        return new ReportCsvResult(fileName, Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
