using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Commands.AddPayment;

/// <summary>
/// Records a payment instalment against a transaction.
/// When the running total of all payments reaches or exceeds <c>FinalAmount</c>,
/// the transaction is automatically transitioned to <see cref="TransactionStatus.Completed"/>
/// and the linked queue entry (if any) is set to Completed.
/// </summary>
public sealed record AddPaymentCommand(
    string TransactionId,
    PaymentMethod PaymentMethod,
    decimal Amount,
    string? ReferenceNumber) : ICommand<string>;
