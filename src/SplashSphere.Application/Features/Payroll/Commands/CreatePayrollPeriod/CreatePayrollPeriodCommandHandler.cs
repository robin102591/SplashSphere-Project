using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.CreatePayrollPeriod;

public sealed class CreatePayrollPeriodCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<CreatePayrollPeriodCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreatePayrollPeriodCommand request,
        CancellationToken cancellationToken)
    {
        // Validate date range: must be weekly (7 days) or semi-monthly boundary
        var daySpan = request.EndDate.DayNumber - request.StartDate.DayNumber + 1;

        if (daySpan == 7)
        {
            // Weekly — ok
        }
        else if (IsValidSemiMonthlyBoundary(request.StartDate, request.EndDate))
        {
            // Semi-monthly — ok
        }
        else
        {
            return Result.Failure<string>(
                Error.Validation("Period must be either 7 days (weekly) or a valid semi-monthly boundary (1st–15th or 16th–last day)."));
        }

        // Check for duplicate: same tenant + branch + start date
        var exists = await db.PayrollPeriods
            .AnyAsync(p => p.BranchId == request.BranchId && p.StartDate == request.StartDate, cancellationToken);

        if (exists)
            return Result.Failure<string>(
                Error.Validation($"A payroll period starting on {request.StartDate} already exists for this branch."));

        // Check for overlapping periods within the same branch
        var overlapping = await db.PayrollPeriods
            .AnyAsync(p => p.BranchId == request.BranchId &&
                           p.StartDate <= request.EndDate && p.EndDate >= request.StartDate, cancellationToken);

        if (overlapping)
            return Result.Failure<string>(
                Error.Validation("This date range overlaps with an existing payroll period for this branch."));

        var year = request.StartDate.Year;
        var cutOffWeek = ComputeCutOffWeek(request.StartDate, request.EndDate);

        var period = new PayrollPeriod(
            tenantContext.TenantId,
            year,
            cutOffWeek,
            request.StartDate,
            request.EndDate,
            request.BranchId);

        db.PayrollPeriods.Add(period);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(period.Id);
    }

    /// <summary>
    /// Checks if the date range represents a valid semi-monthly boundary:
    /// 1st–15th or 16th–last day of the same month.
    /// </summary>
    private static bool IsValidSemiMonthlyBoundary(DateOnly start, DateOnly end)
    {
        if (start.Year != end.Year || start.Month != end.Month)
            return false;

        // First half: 1st–15th
        if (start.Day == 1 && end.Day == 15)
            return true;

        // Second half: 16th–last day
        if (start.Day == 16 && end.Day == DateTime.DaysInMonth(end.Year, end.Month))
            return true;

        return false;
    }

    /// <summary>
    /// Computes the period number for display. Weekly: sequential week in year.
    /// Semi-monthly: period 1–24 within year.
    /// </summary>
    private static int ComputeCutOffWeek(DateOnly startDate, DateOnly endDate)
    {
        var daySpan = endDate.DayNumber - startDate.DayNumber + 1;

        // Semi-monthly: (month - 1) * 2 + half
        if (daySpan > 7)
            return (startDate.Month - 1) * 2 + (startDate.Day <= 15 ? 1 : 2);

        // Weekly: sequential week based on start day
        var jan1 = new DateOnly(startDate.Year, 1, 1);
        var daysUntilStart = ((int)startDate.DayOfWeek - (int)jan1.DayOfWeek + 7) % 7;
        var firstOccurrence = jan1.AddDays(daysUntilStart);

        if (startDate < firstOccurrence)
            return 1;

        return ((startDate.DayNumber - firstOccurrence.DayNumber) / 7) + 1;
    }
}
