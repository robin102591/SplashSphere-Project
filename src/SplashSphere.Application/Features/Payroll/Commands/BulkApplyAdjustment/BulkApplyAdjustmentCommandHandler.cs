using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.BulkApplyAdjustment;

public sealed class BulkApplyAdjustmentCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<BulkApplyAdjustmentCommand, Result>
{
    public async Task<Result> Handle(
        BulkApplyAdjustmentCommand request,
        CancellationToken cancellationToken)
    {
        var entries = await context.PayrollEntries
            .Include(e => e.PayrollPeriod)
            .Include(e => e.Adjustments)
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

        // Validate template if provided
        if (request.TemplateId is not null)
        {
            var templateExists = await context.PayrollAdjustmentTemplates
                .AnyAsync(t => t.Id == request.TemplateId && t.IsActive, cancellationToken);
            if (!templateExists)
                return Result.Failure(Error.Validation("Template not found or is inactive."));
        }

        var category = request.Notes ?? (request.AdjustmentType == AdjustmentType.Bonus ? "Bonus" : "Deduction");

        foreach (var entry in entries)
        {
            var adjustment = new PayrollAdjustment(
                tenantContext.TenantId,
                entry.Id,
                request.AdjustmentType,
                category,
                request.Amount,
                request.Notes,
                request.TemplateId);

            context.PayrollAdjustments.Add(adjustment);
            // EF Core relationship fixup auto-adds to entry.Adjustments
            entry.RecalculateTotals();
        }

        return Result.Success();
    }
}
