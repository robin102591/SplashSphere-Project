using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollEntry;

public sealed class UpdatePayrollEntryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdatePayrollEntryCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePayrollEntryCommand request,
        CancellationToken cancellationToken)
    {
        // Load entry + its parent period in one query
        var entry = await context.PayrollEntries
            .Include(e => e.PayrollPeriod)
            .FirstOrDefaultAsync(e => e.Id == request.EntryId, cancellationToken);

        if (entry is null)
            return Result.Failure(Error.NotFound("PayrollEntry", request.EntryId));

        var periodStatus = entry.PayrollPeriod.Status;

        if (periodStatus == PayrollStatus.Open)
            return Result.Failure(Error.Validation(
                "Payroll entries cannot be modified while the period is Open. Close the period first."));

        if (periodStatus == PayrollStatus.Processed)
            return Result.Failure(Error.Validation(
                "Payroll entries cannot be modified after the period has been Processed. The record is immutable."));

        // Period is Closed — adjustments are allowed
        entry.Bonuses    = request.Bonuses;
        entry.Deductions = request.Deductions;
        entry.Notes      = request.Notes;

        return Result.Success();
    }
}
