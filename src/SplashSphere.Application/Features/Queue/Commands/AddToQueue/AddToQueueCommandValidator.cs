using FluentValidation;

namespace SplashSphere.Application.Features.Queue.Commands.AddToQueue;

public sealed class AddToQueueCommandValidator : AbstractValidator<AddToQueueCommand>
{
    public AddToQueueCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();

        RuleFor(x => x.PlateNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Priority).IsInEnum();

        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
