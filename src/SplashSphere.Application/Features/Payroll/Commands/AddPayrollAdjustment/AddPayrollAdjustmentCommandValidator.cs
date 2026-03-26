using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.AddPayrollAdjustment;

public sealed class AddPayrollAdjustmentCommandValidator : AbstractValidator<AddPayrollAdjustmentCommand>
{
    public AddPayrollAdjustmentCommandValidator()
    {
        RuleFor(x => x.EntryId).NotEmpty();
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Type).IsInEnum();
    }
}
