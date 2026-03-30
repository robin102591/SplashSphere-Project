using System.Globalization;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.AttendanceReports.Queries.ExportAttendanceCsv;

public sealed class ExportAttendanceCsvQueryHandler(IApplicationDbContext context)
    : IRequestHandler<ExportAttendanceCsvQuery, AttendanceCsvResult>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<AttendanceCsvResult> Handle(
        ExportAttendanceCsvQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Attendances
            .AsNoTracking()
            .Where(a => a.Date >= request.From && a.Date <= request.To);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(a => a.Employee.BranchId == request.BranchId);

        if (!string.IsNullOrWhiteSpace(request.EmployeeId))
            query = query.Where(a => a.EmployeeId == request.EmployeeId);

        var records = await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Employee.LastName)
            .ThenBy(a => a.Employee.FirstName)
            .Select(a => new
            {
                EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
                BranchName = a.Employee.Branch.Name,
                a.Date,
                a.TimeIn,
                a.TimeOut,
                a.Notes,
            })
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Employee,Branch,Date,Time In,Time Out,Hours Worked,Notes");

        foreach (var r in records)
        {
            var manilaIn = r.TimeIn.Add(ManilaOffset);
            var manilaOut = r.TimeOut?.Add(ManilaOffset);
            var hours = r.TimeOut.HasValue
                ? (r.TimeOut.Value - r.TimeIn).TotalHours.ToString("F2", CultureInfo.InvariantCulture)
                : "";

            sb.AppendLine(string.Join(",",
                Escape(r.EmployeeName),
                Escape(r.BranchName),
                r.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                manilaIn.ToString("hh:mm tt", CultureInfo.InvariantCulture),
                manilaOut?.ToString("hh:mm tt", CultureInfo.InvariantCulture) ?? "",
                hours,
                Escape(r.Notes ?? "")));
        }

        var fileName = $"attendance_{request.From:yyyyMMdd}_{request.To:yyyyMMdd}.csv";
        return new AttendanceCsvResult(fileName, Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
