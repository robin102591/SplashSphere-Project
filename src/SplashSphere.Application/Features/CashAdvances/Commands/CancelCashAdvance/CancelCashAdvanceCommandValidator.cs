using FluentValidation;

namespace SplashSphere.Application.Features.CashAdvances.Commands.CancelCashAdvance;

public sealed class CancelCashAdvanceCommandValidator : AbstractValidator<CancelCashAdvanceCommand>
{
    public CancelCashAdvanceCommandValidator()
    {
        RuleFor(x => x.CashAdvanceId).NotEmpty();
    }
}
