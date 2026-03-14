using FluentValidation;

namespace SplashSphere.Application.Features.Queue.Commands.MarkNoShow;

public sealed class MarkNoShowCommandValidator : AbstractValidator<MarkNoShowCommand>
{
    public MarkNoShowCommandValidator()
    {
        RuleFor(x => x.QueueEntryId).NotEmpty();
    }
}
