using FluentValidation;

namespace SplashSphere.Application.Features.Shifts.Commands.CloseShift;

public sealed class CloseShiftCommandValidator : AbstractValidator<CloseShiftCommand>
{
    public CloseShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.Denominations)
            .NotEmpty()
            .WithMessage("At least one denomination entry is required.");
        RuleForEach(x => x.Denominations).ChildRules(d =>
        {
            d.RuleFor(x => x.DenominationValue).GreaterThan(0);
            d.RuleFor(x => x.Count).GreaterThanOrEqualTo(0);
        });
    }
}
