using FluentValidation;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Transactions.Commands.UpdateTransactionStatus;

public sealed class UpdateTransactionStatusCommandValidator : AbstractValidator<UpdateTransactionStatusCommand>
{
    // The set of statuses that are valid targets for a manual status change.
    // Pending is only the initial state (set by CreateTransaction) — it cannot
    // be set manually. NoShow/Called/InService are queue states, not transaction states.
    private static readonly HashSet<TransactionStatus> AllowedTargetStatuses =
    [
        TransactionStatus.InProgress,
        TransactionStatus.Completed,
        TransactionStatus.Cancelled,
        TransactionStatus.Refunded,
    ];

    public UpdateTransactionStatusCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Transaction ID is required.");

        RuleFor(x => x.NewStatus)
            .Must(s => AllowedTargetStatuses.Contains(s))
            .WithMessage(x =>
                $"'{x.NewStatus}' is not a valid target status. " +
                $"Allowed: {string.Join(", ", AllowedTargetStatuses)}.");
    }
}
