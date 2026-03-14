using FluentValidation;

namespace SplashSphere.Application.Features.Queue.Commands.RequeueEntry;

public sealed class RequeueEntryCommandValidator : AbstractValidator<RequeueEntryCommand>
{
    public RequeueEntryCommandValidator()
    {
        RuleFor(x => x.QueueEntryId).NotEmpty();

        RuleFor(x => x.NewPriority)
            .IsInEnum()
            .When(x => x.NewPriority.HasValue);
    }
}
