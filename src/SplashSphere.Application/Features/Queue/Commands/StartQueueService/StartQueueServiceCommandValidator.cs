using FluentValidation;

namespace SplashSphere.Application.Features.Queue.Commands.StartQueueService;

public sealed class StartQueueServiceCommandValidator : AbstractValidator<StartQueueServiceCommand>
{
    public StartQueueServiceCommandValidator()
    {
        RuleFor(x => x.QueueEntryId).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
    }
}
