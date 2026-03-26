using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.DeletePayrollAdjustment;

public sealed class DeletePayrollAdjustmentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeletePayrollAdjustmentCommand, Result>
{
    public async Task<Result> Handle(
        DeletePayrollAdjustmentCommand request,
        CancellationToken cancellationToken)
    {
        var adjustment = await context.PayrollAdjustments
            .Include(a => a.Entry)
                .ThenInclude(e => e.PayrollPeriod)
            .Include(a => a.Entry)
                .ThenInclude(e => e.Adjustments)
            .FirstOrDefaultAsync(a => a.Id == request.AdjustmentId, cancellationToken);

        if (adjustment is null)
            return Result.Failure(Error.NotFound("PayrollAdjustment", request.AdjustmentId));

        if (adjustment.Entry.PayrollPeriod.Status != PayrollStatus.Closed)
            return Result.Failure(Error.Validation(
                "Adjustments can only be removed when the period is Closed."));

        var entry = adjustment.Entry;
        context.PayrollAdjustments.Remove(adjustment);
        entry.Adjustments.Remove(adjustment);
        entry.RecalculateTotals();

        return Result.Success();
    }
}
