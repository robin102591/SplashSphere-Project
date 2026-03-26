using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.CreatePayrollTemplate;

public sealed class CreatePayrollTemplateCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreatePayrollTemplateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreatePayrollTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var duplicate = await context.PayrollAdjustmentTemplates
            .AnyAsync(t => t.Name == request.Name, cancellationToken);

        if (duplicate)
            return Result.Failure<string>(Error.Validation(
                $"A template named '{request.Name}' already exists."));

        var template = new PayrollAdjustmentTemplate(
            tenantContext.TenantId,
            request.Name,
            request.Type,
            request.DefaultAmount);

        context.PayrollAdjustmentTemplates.Add(template);

        return Result.Success(template.Id);
    }
}
