using FluentValidation;

namespace SplashSphere.Application.Features.Queue.Commands.CancelQueueEntry;

public sealed class CancelQueueEntryCommandValidator : AbstractValidator<CancelQueueEntryCommand>
{
    public CancelQueueEntryCommandValidator()
    {
        RuleFor(x => x.QueueEntryId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
