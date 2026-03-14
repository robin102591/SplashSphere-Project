using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Transactions.Queries.GetTransactionById;

/// <summary>Returns the full detail of a single transaction including all line items, assignments, and payments.</summary>
public sealed record GetTransactionByIdQuery(string TransactionId) : IQuery<TransactionDetailDto?>;
