using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.AddPayrollAdjustment;

public sealed class AddPayrollAdjustmentCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<AddPayrollAdjustmentCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        AddPayrollAdjustmentCommand request,
        CancellationToken cancellationToken)
    {
        var entry = await context.PayrollEntries
            .Include(e => e.PayrollPeriod)
            .Include(e => e.Adjustments)
            .FirstOrDefaultAsync(e => e.Id == request.EntryId, cancellationToken);

        if (entry is null)
            return Result.Failure<string>(Error.NotFound("PayrollEntry", request.EntryId));

        if (entry.PayrollPeriod.Status != PayrollStatus.Closed)
            return Result.Failure<string>(Error.Validation(
                "Adjustments can only be added when the period is Closed."));

        if (request.TemplateId is not null)
        {
            var templateExists = await context.PayrollAdjustmentTemplates
                .AnyAsync(t => t.Id == request.TemplateId && t.IsActive, cancellationToken);
            if (!templateExists)
                return Result.Failure<string>(Error.Validation("Template not found or is inactive."));
        }

        var adjustment = new PayrollAdjustment(
            tenantContext.TenantId,
            entry.Id,
            request.Type,
            request.Category,
            request.Amount,
            request.Notes,
            request.TemplateId);

        context.PayrollAdjustments.Add(adjustment);
        // EF Core relationship fixup auto-adds to entry.Adjustments
        entry.RecalculateTotals();

        return Result.Success(adjustment.Id);
    }
}
