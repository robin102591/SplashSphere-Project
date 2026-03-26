using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollAdjustment;

public sealed class UpdatePayrollAdjustmentCommandValidator : AbstractValidator<UpdatePayrollAdjustmentCommand>
{
    public UpdatePayrollAdjustmentCommandValidator()
    {
        RuleFor(x => x.AdjustmentId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
