using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.ProcessPayrollPeriod;

public sealed class ProcessPayrollPeriodCommandValidator : AbstractValidator<ProcessPayrollPeriodCommand>
{
    public ProcessPayrollPeriodCommandValidator()
    {
        RuleFor(x => x.PeriodId).NotEmpty().WithMessage("Period ID is required.");
    }
}
