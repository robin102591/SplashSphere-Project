using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.DeletePayrollAdjustment;

public sealed class DeletePayrollAdjustmentCommandValidator : AbstractValidator<DeletePayrollAdjustmentCommand>
{
    public DeletePayrollAdjustmentCommandValidator()
    {
        RuleFor(x => x.AdjustmentId).NotEmpty().WithMessage("Adjustment ID is required.");
    }
}
