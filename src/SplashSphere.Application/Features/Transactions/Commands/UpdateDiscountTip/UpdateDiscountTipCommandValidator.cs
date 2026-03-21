using FluentValidation;

namespace SplashSphere.Application.Features.Transactions.Commands.UpdateDiscountTip;

public sealed class UpdateDiscountTipCommandValidator : AbstractValidator<UpdateDiscountTipCommand>
{
    public UpdateDiscountTipCommandValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TipAmount).GreaterThanOrEqualTo(0);
    }
}
