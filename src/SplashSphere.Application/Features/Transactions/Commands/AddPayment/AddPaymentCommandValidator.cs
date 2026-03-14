using FluentValidation;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Transactions.Commands.AddPayment;

public sealed class AddPaymentCommandValidator : AbstractValidator<AddPaymentCommand>
{
    public AddPaymentCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Transaction ID is required.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");

        // Reference number is required for all non-cash methods
        RuleFor(x => x.ReferenceNumber)
            .NotEmpty()
            .WithMessage("Reference number is required for non-cash payments.")
            .When(x => x.PaymentMethod != PaymentMethod.Cash);
    }
}
