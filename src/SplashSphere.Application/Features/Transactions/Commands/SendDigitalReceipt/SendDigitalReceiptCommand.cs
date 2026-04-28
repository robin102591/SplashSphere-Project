using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Transactions.Commands.SendDigitalReceipt;

/// <summary>
/// Manually re-sends the HTML email receipt for an already-completed
/// transaction. Used by the cashier when (a) the customer didn't receive
/// the auto-send email (spam folder, typo previously corrected), or
/// (b) the transaction predates the digital-receipt feature.
/// </summary>
/// <remarks>
/// Reuses the same plan-gate, render, and send path as the auto-send
/// handler so behaviour stays consistent. Differs in two ways:
///   - Returns explicit Result errors instead of swallowing — the cashier
///     needs to know whether the send succeeded.
///   - Honors a caller-supplied <see cref="OverrideEmail"/> so the cashier
///     can resend to a corrected address without making the customer
///     update their profile first.
/// </remarks>
public sealed record SendDigitalReceiptCommand(
    string TransactionId,
    string? OverrideEmail = null) : ICommand;
