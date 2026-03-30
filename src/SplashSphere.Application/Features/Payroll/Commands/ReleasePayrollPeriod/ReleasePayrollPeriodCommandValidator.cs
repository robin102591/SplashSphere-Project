using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.ReleasePayrollPeriod;

public sealed class ReleasePayrollPeriodCommandValidator : AbstractValidator<ReleasePayrollPeriodCommand>
{
    public ReleasePayrollPeriodCommandValidator()
    {
        RuleFor(x => x.PeriodId).NotEmpty().WithMessage("Period ID is required.");
    }
}
