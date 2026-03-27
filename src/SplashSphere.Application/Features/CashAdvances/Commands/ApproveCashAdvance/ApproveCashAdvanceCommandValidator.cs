using FluentValidation;

namespace SplashSphere.Application.Features.CashAdvances.Commands.ApproveCashAdvance;

public sealed class ApproveCashAdvanceCommandValidator : AbstractValidator<ApproveCashAdvanceCommand>
{
    public ApproveCashAdvanceCommandValidator()
    {
        RuleFor(x => x.CashAdvanceId).NotEmpty();
    }
}
