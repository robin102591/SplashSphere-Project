using FluentValidation;

namespace SplashSphere.Application.Features.Shifts.Commands.OpenShift;

public sealed class OpenShiftCommandValidator : AbstractValidator<OpenShiftCommand>
{
    public OpenShiftCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.OpeningCashFund).GreaterThanOrEqualTo(0);
    }
}
