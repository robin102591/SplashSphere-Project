using FluentValidation;

namespace SplashSphere.Application.Features.Shifts.Commands.RecordCashMovement;

public sealed class RecordCashMovementCommandValidator : AbstractValidator<RecordCashMovementCommand>
{
    public RecordCashMovementCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Reference).MaximumLength(256).When(x => x.Reference is not null);
    }
}
