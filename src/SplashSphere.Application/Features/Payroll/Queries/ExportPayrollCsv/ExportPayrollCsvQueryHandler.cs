using System.Globalization;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Queries.ExportPayrollCsv;

public sealed class ExportPayrollCsvQueryHandler(IApplicationDbContext context)
    : IRequestHandler<ExportPayrollCsvQuery, PayrollCsvResult?>
{
    public async Task<PayrollCsvResult?> Handle(
        ExportPayrollCsvQuery request,
        CancellationToken cancellationToken)
    {
        var period = await context.PayrollPeriods
            .AsNoTracking()
            .Where(p => p.Id == request.PeriodId)
            .Select(p => new { p.StartDate, p.EndDate, p.Year, p.CutOffWeek })
            .FirstOrDefaultAsync(cancellationToken);

        if (period is null)
            return null;

        var entries = await context.PayrollEntries
            .AsNoTracking()
            .Where(e => e.PayrollPeriodId == request.PeriodId)
            .OrderBy(e => e.Employee.LastName)
            .ThenBy(e => e.Employee.FirstName)
            .Select(e => new
            {
                EmployeeName = e.Employee.FirstName + " " + e.Employee.LastName,
                Branch = e.Employee.Branch.Name,
                EmployeeType = e.EmployeeTypeSnapshot.ToString(),
                e.DaysWorked,
                e.DailyRateSnapshot,
                e.BaseSalary,
                e.TotalCommissions,
                e.TotalTips,
                e.Bonuses,
                e.Deductions,
                NetPay = e.BaseSalary + e.TotalCommissions + e.Bonuses - e.Deductions,
                e.Notes,
            })
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Employee,Branch,Type,Days Worked,Daily Rate,Base Salary,Commissions,Tips (Paid),Bonuses,Deductions,Net Pay,Notes");

        foreach (var e in entries)
        {
            sb.AppendLine(string.Join(",",
                Escape(e.EmployeeName),
                Escape(e.Branch),
                Escape(e.EmployeeType),
                e.DaysWorked.ToString(CultureInfo.InvariantCulture),
                (e.DailyRateSnapshot ?? 0m).ToString("F2", CultureInfo.InvariantCulture),
                e.BaseSalary.ToString("F2", CultureInfo.InvariantCulture),
                e.TotalCommissions.ToString("F2", CultureInfo.InvariantCulture),
                e.TotalTips.ToString("F2", CultureInfo.InvariantCulture),
                e.Bonuses.ToString("F2", CultureInfo.InvariantCulture),
                e.Deductions.ToString("F2", CultureInfo.InvariantCulture),
                e.NetPay.ToString("F2", CultureInfo.InvariantCulture),
                Escape(e.Notes ?? "")));
        }

        var fileName = $"payroll_{period.StartDate:yyyyMMdd}_{period.EndDate:yyyyMMdd}.csv";
        return new PayrollCsvResult(fileName, Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
