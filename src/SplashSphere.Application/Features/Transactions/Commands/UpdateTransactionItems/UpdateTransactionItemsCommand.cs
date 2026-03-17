using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Transactions.Commands.CreateTransaction;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Commands.UpdateTransactionItems;

/// <summary>
/// Replaces all line items (services, packages, merchandise) on an InProgress transaction.
/// Only allowed before any payments have been recorded.
/// Returns a failure if the transaction is not InProgress or has existing payments.
/// </summary>
public sealed record UpdateTransactionItemsCommand(
    string TransactionId,
    IReadOnlyList<TransactionServiceRequest> Services,
    IReadOnlyList<TransactionPackageRequest> Packages,
    IReadOnlyList<TransactionMerchandiseRequest> Merchandise,
    decimal DiscountAmount,
    string? Notes) : ICommand;
