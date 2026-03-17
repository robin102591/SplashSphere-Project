using FluentValidation;

namespace SplashSphere.Application.Features.Transactions.Commands.UpdateTransactionItems;

public sealed class UpdateTransactionItemsCommandValidator
    : AbstractValidator<UpdateTransactionItemsCommand>
{
    public UpdateTransactionItemsCommandValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Services).NotNull();
        RuleFor(x => x.Packages).NotNull();
        RuleFor(x => x.Merchandise).NotNull();
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.Services.Count + x.Packages.Count > 0)
            .WithMessage("At least one service or package is required.")
            .OverridePropertyName("Services");
    }
}
