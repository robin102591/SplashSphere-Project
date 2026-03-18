using FluentValidation;

namespace SplashSphere.Application.Features.Transactions.Commands.RefundTransaction;

public sealed class RefundTransactionCommandValidator : AbstractValidator<RefundTransactionCommand>
{
    public RefundTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Transaction ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Refund reason cannot exceed 500 characters.")
            .When(x => x.Reason is not null);
    }
}
