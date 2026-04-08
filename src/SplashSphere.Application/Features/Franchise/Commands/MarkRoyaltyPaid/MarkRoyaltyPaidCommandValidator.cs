using FluentValidation;

namespace SplashSphere.Application.Features.Franchise.Commands.MarkRoyaltyPaid;

public sealed class MarkRoyaltyPaidCommandValidator : AbstractValidator<MarkRoyaltyPaidCommand>
{
    public MarkRoyaltyPaidCommandValidator()
    {
        RuleFor(x => x.RoyaltyPeriodId).NotEmpty();
    }
}
