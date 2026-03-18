using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Commands.RefundTransaction;

/// <summary>
/// Refunds a <see cref="TransactionStatus.Completed"/> transaction.
/// <list type="bullet">
///   <item>Claws back commission amounts from each employee's <see cref="PayrollEntry"/>
///   in the payroll period that covers the transaction's completion date,
///   provided the period has not yet been <see cref="PayrollStatus.Processed"/>.</item>
///   <item>Restores merchandise stock quantities.</item>
///   <item>Transitions transaction status to <see cref="TransactionStatus.Refunded"/>.</item>
/// </list>
/// </summary>
public sealed record RefundTransactionCommand(
    string TransactionId,
    string? Reason
) : ICommand;
