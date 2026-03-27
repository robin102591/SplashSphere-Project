using FluentValidation;

namespace SplashSphere.Application.Features.CashAdvances.Commands.CreateCashAdvance;

public sealed class CreateCashAdvanceCommandValidator : AbstractValidator<CreateCashAdvanceCommand>
{
    public CreateCashAdvanceCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.DeductionPerPeriod)
            .GreaterThan(0)
            .LessThanOrEqualTo(x => x.Amount)
            .WithMessage("Deduction per period must not exceed the advance amount.");
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
