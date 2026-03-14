using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.ClosePayrollPeriod;

public sealed class ClosePayrollPeriodCommandValidator : AbstractValidator<ClosePayrollPeriodCommand>
{
    public ClosePayrollPeriodCommandValidator()
    {
        RuleFor(x => x.PeriodId).NotEmpty().WithMessage("Period ID is required.");
    }
}
