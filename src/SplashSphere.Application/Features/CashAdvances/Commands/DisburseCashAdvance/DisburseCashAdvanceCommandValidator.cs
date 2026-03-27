using FluentValidation;

namespace SplashSphere.Application.Features.CashAdvances.Commands.DisburseCashAdvance;

public sealed class DisburseCashAdvanceCommandValidator : AbstractValidator<DisburseCashAdvanceCommand>
{
    public DisburseCashAdvanceCommandValidator()
    {
        RuleFor(x => x.CashAdvanceId).NotEmpty();
    }
}
