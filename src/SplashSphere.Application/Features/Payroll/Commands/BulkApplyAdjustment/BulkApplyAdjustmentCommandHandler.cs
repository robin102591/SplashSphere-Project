using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.BulkApplyAdjustment;

public sealed class BulkApplyAdjustmentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<BulkApplyAdjustmentCommand, Result>
{
    public async Task<Result> Handle(
        BulkApplyAdjustmentCommand request,
        CancellationToken cancellationToken)
    {
        var entries = await context.PayrollEntries
            .Include(e => e.PayrollPeriod)
            .Where(e => request.EntryIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        if (entries.Count != request.EntryIds.Count)
            return Result.Failure(Error.Validation(
                "One or more selected entries were not found."));

        // All entries must belong to the same period
        var periodIds = entries.Select(e => e.PayrollPeriodId).Distinct().ToList();
        if (periodIds.Count > 1)
            return Result.Failure(Error.Validation(
                "All selected entries must belong to the same payroll period."));

        var period = entries[0].PayrollPeriod;
        if (period.Status != PayrollStatus.Closed)
            return Result.Failure(Error.Validation(
                $"Entries can only be adjusted when the period is Closed. Current status: '{period.Status}'."));

        foreach (var entry in entries)
        {
            if (request.AdjustmentType == AdjustmentType.Bonus)
                entry.Bonuses += request.Amount;
            else
                entry.Deductions += request.Amount;

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                entry.Notes = string.IsNullOrEmpty(entry.Notes)
                    ? request.Notes
                    : $"{entry.Notes}; {request.Notes}";
            }
        }

        return Result.Success();
    }
}
