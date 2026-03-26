using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.DeletePayrollTemplate;

public sealed class DeletePayrollTemplateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeletePayrollTemplateCommand, Result>
{
    public async Task<Result> Handle(
        DeletePayrollTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var template = await context.PayrollAdjustmentTemplates
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template is null)
            return Result.Failure(Error.NotFound("PayrollAdjustmentTemplate", request.Id));

        // Soft delete — toggle IsActive
        template.IsActive = !template.IsActive;

        return Result.Success();
    }
}
