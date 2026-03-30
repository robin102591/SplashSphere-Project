using System.Globalization;
using System.Text;
using MediatR;
using SplashSphere.Application.Features.Reports.Queries.GetCommissionsReport;

namespace SplashSphere.Application.Features.Reports.Queries.ExportCommissionsCsv;

public sealed class ExportCommissionsCsvQueryHandler(ISender sender)
    : IRequestHandler<ExportCommissionsCsvQuery, ReportCsvResult>
{
    public async Task<ReportCsvResult> Handle(
        ExportCommissionsCsvQuery request,
        CancellationToken cancellationToken)
    {
        var report = await sender.Send(
            new GetCommissionsReportQuery(request.From, request.To, request.BranchId, request.EmployeeId),
            cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Employee,Branch,Type,Total Commissions,Transactions");

        foreach (var e in report.Employees)
        {
            sb.AppendLine(string.Join(",",
                Escape(e.EmployeeName),
                Escape(e.BranchName),
                Escape(e.EmployeeType),
                e.TotalCommissions.ToString("F2", CultureInfo.InvariantCulture),
                e.TransactionCount.ToString(CultureInfo.InvariantCulture)));
        }

        var fileName = $"commissions_{request.From:yyyyMMdd}_{request.To:yyyyMMdd}.csv";
        return new ReportCsvResult(fileName, Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
