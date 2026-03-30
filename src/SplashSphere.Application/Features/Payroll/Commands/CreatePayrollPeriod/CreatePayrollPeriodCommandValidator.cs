using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.CreatePayrollPeriod;

public sealed class CreatePayrollPeriodCommandValidator : AbstractValidator<CreatePayrollPeriodCommand>
{
    public CreatePayrollPeriodCommandValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .Must((cmd, endDate) => endDate > cmd.StartDate)
            .WithMessage("EndDate must be after StartDate.");
    }
}
