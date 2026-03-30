using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.AttendanceReports.Queries.GetAttendanceReport;

public sealed class GetAttendanceReportQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAttendanceReportQuery, AttendanceReportDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);
    private static readonly TimeSpan DefaultExpectedStart = new(8, 0, 0);
    private static readonly TimeSpan DefaultExpectedEnd = new(17, 0, 0);

    public async Task<AttendanceReportDto> Handle(
        GetAttendanceReportQuery request,
        CancellationToken cancellationToken)
    {
        // ── 1. Load employees ────────────────────────────────────────────────
        var employeeQuery = context.Employees
            .AsNoTracking()
            .Where(e => e.IsActive);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            employeeQuery = employeeQuery.Where(e => e.BranchId == request.BranchId);

        if (!string.IsNullOrWhiteSpace(request.EmployeeId))
            employeeQuery = employeeQuery.Where(e => e.Id == request.EmployeeId);

        var employees = await employeeQuery
            .Select(e => new
            {
                e.Id,
                Name = e.FirstName + " " + e.LastName,
                BranchName = e.Branch.Name,
                EmployeeType = e.EmployeeType.ToString(),
            })
            .ToListAsync(cancellationToken);

        if (employees.Count == 0)
            return EmptyReport(request);

        var employeeIds = employees.Select(e => e.Id).ToHashSet();

        // ── 2. Load attendance records ───────────────────────────────────────
        var records = await context.Attendances
            .AsNoTracking()
            .Where(a => a.Date >= request.From && a.Date <= request.To)
            .Where(a => employeeIds.Contains(a.EmployeeId))
            .Select(a => new
            {
                a.EmployeeId,
                a.Date,
                a.TimeIn,
                a.TimeOut,
            })
            .ToListAsync(cancellationToken);

        var grouped = records
            .GroupBy(r => r.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ── 3. Calculate expected work days ──────────────────────────────────
        var expectedWorkDays = CountExpectedWorkDays(
            request.From, request.To, request.ExpectedWorkDaysPerWeek);

        // ── 4. Build per-employee rows ───────────────────────────────────────
        var rows = new List<EmployeeAttendanceRow>(employees.Count);
        var totalLate = 0;
        decimal totalHoursAll = 0;
        var totalDaysPresentAll = 0;

        foreach (var emp in employees)
        {
            if (!grouped.TryGetValue(emp.Id, out var empRecords))
            {
                rows.Add(new EmployeeAttendanceRow(
                    emp.Id, emp.Name, emp.BranchName, emp.EmployeeType,
                    DaysPresent: 0, DaysAbsent: expectedWorkDays,
                    LateCount: 0, EarlyOutCount: 0,
                    TotalHours: 0, AverageHoursPerDay: 0));
                continue;
            }

            var daysPresent = empRecords.Count;
            var daysAbsent = Math.Max(0, expectedWorkDays - daysPresent);

            var lateCount = 0;
            var earlyOutCount = 0;
            decimal totalHours = 0;

            foreach (var r in empRecords)
            {
                var manilaTimeIn = r.TimeIn.Add(ManilaOffset).TimeOfDay;
                if (manilaTimeIn > DefaultExpectedStart)
                    lateCount++;

                if (r.TimeOut.HasValue)
                {
                    var manilaTimeOut = r.TimeOut.Value.Add(ManilaOffset).TimeOfDay;
                    if (manilaTimeOut < DefaultExpectedEnd)
                        earlyOutCount++;

                    totalHours += (decimal)(r.TimeOut.Value - r.TimeIn).TotalHours;
                }
            }

            var avgHours = daysPresent > 0
                ? Math.Round(totalHours / daysPresent, 1, MidpointRounding.AwayFromZero)
                : 0;

            totalLate += lateCount;
            totalHoursAll += totalHours;
            totalDaysPresentAll += daysPresent;

            rows.Add(new EmployeeAttendanceRow(
                emp.Id, emp.Name, emp.BranchName, emp.EmployeeType,
                daysPresent, daysAbsent, lateCount, earlyOutCount,
                Math.Round(totalHours, 1, MidpointRounding.AwayFromZero),
                avgHours));
        }

        // ── 5. Build summary ─────────────────────────────────────────────────
        var avgAttendanceRate = expectedWorkDays > 0 && employees.Count > 0
            ? Math.Round(
                (decimal)totalDaysPresentAll / (employees.Count * expectedWorkDays) * 100,
                1, MidpointRounding.AwayFromZero)
            : 0;

        var avgHoursPerDay = totalDaysPresentAll > 0
            ? Math.Round(totalHoursAll / totalDaysPresentAll, 1, MidpointRounding.AwayFromZero)
            : 0;

        var summary = new AttendanceReportSummary(
            employees.Count,
            avgAttendanceRate,
            totalLate,
            avgHoursPerDay);

        return new AttendanceReportDto(
            request.From, request.To, request.BranchId, request.EmployeeId,
            summary, rows);
    }

    /// <summary>
    /// Count expected work days in the range. For a 6-day week, excludes Sundays.
    /// For a 5-day week, excludes Saturday and Sunday.
    /// </summary>
    private static int CountExpectedWorkDays(DateOnly from, DateOnly to, int workDaysPerWeek)
    {
        var count = 0;
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            var dow = d.DayOfWeek;
            if (workDaysPerWeek >= 7 || (workDaysPerWeek == 6 && dow != DayOfWeek.Sunday) ||
                (workDaysPerWeek <= 5 && dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday))
                count++;
        }
        return count;
    }

    private static AttendanceReportDto EmptyReport(GetAttendanceReportQuery request) =>
        new(request.From, request.To, request.BranchId, request.EmployeeId,
            new AttendanceReportSummary(0, 0, 0, 0), []);
}
