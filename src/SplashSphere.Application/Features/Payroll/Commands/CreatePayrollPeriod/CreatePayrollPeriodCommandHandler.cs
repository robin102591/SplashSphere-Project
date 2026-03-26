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
        // Check for duplicate: same tenant + same start date
        var exists = await db.PayrollPeriods
            .AnyAsync(p => p.StartDate == request.StartDate, cancellationToken);

        if (exists)
            return Result.Failure<string>(
                Error.Validation($"A payroll period starting on {request.StartDate} already exists."));

        // Check for overlapping periods
        var overlapping = await db.PayrollPeriods
            .AnyAsync(p => p.StartDate <= request.EndDate && p.EndDate >= request.StartDate, cancellationToken);

        if (overlapping)
            return Result.Failure<string>(
                Error.Validation("This date range overlaps with an existing payroll period."));

        var year = request.StartDate.Year;
        var cutOffWeek = ComputeCutOffWeek(request.StartDate);

        var period = new PayrollPeriod(
            tenantContext.TenantId,
            year,
            cutOffWeek,
            request.StartDate,
            request.EndDate);

        db.PayrollPeriods.Add(period);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(period.Id);
    }

    /// <summary>
    /// Computes a sequential week number within the year based on the start date.
    /// </summary>
    private static int ComputeCutOffWeek(DateOnly startDate)
    {
        var jan1 = new DateOnly(startDate.Year, 1, 1);
        var daysUntilStart = ((int)startDate.DayOfWeek - (int)jan1.DayOfWeek + 7) % 7;
        var firstOccurrence = jan1.AddDays(daysUntilStart);

        if (startDate < firstOccurrence)
            return 1;

        return ((startDate.DayNumber - firstOccurrence.DayNumber) / 7) + 1;
    }
}
