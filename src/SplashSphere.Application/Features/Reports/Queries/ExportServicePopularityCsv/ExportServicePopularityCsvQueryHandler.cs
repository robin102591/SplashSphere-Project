using System.Globalization;
using System.Text;
using MediatR;
using SplashSphere.Application.Features.Reports.Queries.GetServicePopularityReport;

namespace SplashSphere.Application.Features.Reports.Queries.ExportServicePopularityCsv;

public sealed class ExportServicePopularityCsvQueryHandler(ISender sender)
    : IRequestHandler<ExportServicePopularityCsvQuery, ReportCsvResult>
{
    public async Task<ReportCsvResult> Handle(
        ExportServicePopularityCsvQuery request,
        CancellationToken cancellationToken)
    {
        var report = await sender.Send(
            new GetServicePopularityReportQuery(request.From, request.To, request.BranchId, request.Top),
            cancellationToken);

        var sb = new StringBuilder();

        sb.AppendLine("Type,Name,Category,Times Performed,Total Revenue,Average Revenue");

        foreach (var s in report.Services)
        {
            sb.AppendLine(string.Join(",",
                "Service",
                Escape(s.ServiceName),
                Escape(s.CategoryName ?? ""),
                s.TimesPerformed.ToString(CultureInfo.InvariantCulture),
                s.TotalRevenue.ToString("F2", CultureInfo.InvariantCulture),
                s.AverageRevenue.ToString("F2", CultureInfo.InvariantCulture)));
        }

        foreach (var p in report.Packages)
        {
            sb.AppendLine(string.Join(",",
                "Package",
                Escape(p.PackageName),
                "",
                p.TimesPerformed.ToString(CultureInfo.InvariantCulture),
                p.TotalRevenue.ToString("F2", CultureInfo.InvariantCulture),
                p.AverageRevenue.ToString("F2", CultureInfo.InvariantCulture)));
        }

        var fileName = $"service_popularity_{request.From:yyyyMMdd}_{request.To:yyyyMMdd}.csv";
        return new ReportCsvResult(fileName, Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
