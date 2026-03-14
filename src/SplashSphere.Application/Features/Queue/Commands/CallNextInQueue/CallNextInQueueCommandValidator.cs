using FluentValidation;

namespace SplashSphere.Application.Features.Queue.Commands.CallNextInQueue;

public sealed class CallNextInQueueCommandValidator : AbstractValidator<CallNextInQueueCommand>
{
    public CallNextInQueueCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
    }
}
