using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.ReleasePayrollPeriod;

public sealed class ReleasePayrollPeriodCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IEventPublisher eventPublisher)
    : IRequestHandler<ReleasePayrollPeriodCommand, Result>
{
    public async Task<Result> Handle(
        ReleasePayrollPeriodCommand request,
        CancellationToken cancellationToken)
    {
        var period = await context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == request.PeriodId, cancellationToken);

        if (period is null)
            return Result.Failure(Error.NotFound("PayrollPeriod", request.PeriodId));

        if (period.Status != PayrollStatus.Processed)
            return Result.Failure(Error.Validation(
                $"Only Processed periods can be released. Current status: '{period.Status}'."));

        var entryCount = await context.PayrollEntries
            .AsNoTracking()
            .CountAsync(e => e.PayrollPeriodId == request.PeriodId, cancellationToken);

        var totalNetPay = await context.PayrollEntries
            .AsNoTracking()
            .Where(e => e.PayrollPeriodId == request.PeriodId)
            .SumAsync(e => e.BaseSalary + e.TotalCommissions + e.Bonuses - e.Deductions,
                cancellationToken);

        period.Status = PayrollStatus.Released;
        period.ReleasedAt = DateTime.UtcNow;

        eventPublisher.Enqueue(new PayrollReleasedEvent(
            period.Id,
            tenantContext.TenantId,
            period.Year,
            period.CutOffWeek,
            totalNetPay,
            entryCount));

        return Result.Success();
    }
}
