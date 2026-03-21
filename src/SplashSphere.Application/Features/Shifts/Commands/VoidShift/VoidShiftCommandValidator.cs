using FluentValidation;

namespace SplashSphere.Application.Features.Shifts.Commands.VoidShift;

public sealed class VoidShiftCommandValidator : AbstractValidator<VoidShiftCommand>
{
    public VoidShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
