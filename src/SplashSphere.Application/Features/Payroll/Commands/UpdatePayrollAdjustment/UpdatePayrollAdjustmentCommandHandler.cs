using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollAdjustment;

public sealed class UpdatePayrollAdjustmentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdatePayrollAdjustmentCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePayrollAdjustmentCommand request,
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
                "Adjustments can only be modified when the period is Closed."));

        adjustment.Amount = request.Amount;
        adjustment.Notes = request.Notes;
        adjustment.Entry.RecalculateTotals();

        return Result.Success();
    }
}
