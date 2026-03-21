using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Commands.UpdateDiscountTip;

/// <summary>
/// Updates the discount and/or tip on a Pending or InProgress transaction.
/// Recalculates FinalAmount = TotalAmount - DiscountAmount + TaxAmount.
/// Validates that existing payments do not exceed the new customer total.
/// </summary>
public sealed record UpdateDiscountTipCommand(
    string TransactionId,
    decimal DiscountAmount,
    decimal TipAmount) : ICommand;
