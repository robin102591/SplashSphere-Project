using FluentValidation;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Loyalty.Commands.CreateLoyaltyReward;

public sealed class CreateLoyaltyRewardCommandValidator : AbstractValidator<CreateLoyaltyRewardCommand>
{
    public CreateLoyaltyRewardCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PointsCost).GreaterThan(0);

        RuleFor(x => x.ServiceId).NotEmpty()
            .When(x => x.RewardType == RewardType.FreeService)
            .WithMessage("ServiceId is required for FreeService rewards.");

        RuleFor(x => x.PackageId).NotEmpty()
            .When(x => x.RewardType == RewardType.FreePackage)
            .WithMessage("PackageId is required for FreePackage rewards.");

        RuleFor(x => x.DiscountAmount).GreaterThan(0)
            .When(x => x.RewardType == RewardType.DiscountAmount)
            .WithMessage("DiscountAmount must be greater than 0.");

        RuleFor(x => x.DiscountPercent).InclusiveBetween(1, 100)
            .When(x => x.RewardType == RewardType.DiscountPercent)
            .WithMessage("DiscountPercent must be between 1 and 100.");
    }
}
