using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Transactions.Queries.GetReceipt;

/// <summary>
/// Returns a receipt-formatted view of a completed transaction.
/// Suitable for printing or displaying to the customer.
/// </summary>
public sealed record GetReceiptQuery(string TransactionId) : IQuery<ReceiptDto?>;
