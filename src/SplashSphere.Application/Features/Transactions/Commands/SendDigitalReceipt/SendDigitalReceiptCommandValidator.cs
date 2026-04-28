using FluentValidation;

namespace SplashSphere.Application.Features.Transactions.Commands.SendDigitalReceipt;

public sealed class SendDigitalReceiptCommandValidator : AbstractValidator<SendDigitalReceiptCommand>
{
    public SendDigitalReceiptCommandValidator()
    {
        RuleFor(c => c.TransactionId)
            .NotEmpty();

        // Empty/null OverrideEmail = use the customer's on-file email; that's
        // a valid call. We only validate the format when one is supplied.
        RuleFor(c => c.OverrideEmail)
            .EmailAddress()
            .When(c => !string.IsNullOrWhiteSpace(c.OverrideEmail))
            .WithMessage("Override email must be a valid address.");
    }
}
