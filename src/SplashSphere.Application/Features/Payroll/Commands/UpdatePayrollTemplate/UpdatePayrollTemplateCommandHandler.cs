using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollTemplate;

public sealed class UpdatePayrollTemplateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdatePayrollTemplateCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePayrollTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var template = await context.PayrollAdjustmentTemplates
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template is null)
            return Result.Failure(Error.NotFound("PayrollAdjustmentTemplate", request.Id));

        // Check duplicate name (excluding self)
        var duplicate = await context.PayrollAdjustmentTemplates
            .AnyAsync(t => t.Name == request.Name && t.Id != request.Id, cancellationToken);

        if (duplicate)
            return Result.Failure(Error.Validation(
                $"A template named '{request.Name}' already exists."));

        template.Name = request.Name;
        template.Type = request.Type;
        template.DefaultAmount = request.DefaultAmount;
        template.SortOrder = request.SortOrder;

        return Result.Success();
    }
}
