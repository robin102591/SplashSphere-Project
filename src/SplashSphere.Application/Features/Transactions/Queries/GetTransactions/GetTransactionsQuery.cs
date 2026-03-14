using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Queries.GetTransactions;

/// <summary>Returns a paginated list of transactions for a branch, with optional filters.</summary>
public sealed record GetTransactionsQuery(
    string BranchId,
    int Page = 1,
    int PageSize = 20,
    TransactionStatus? Status = null,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    string? Search = null) : IQuery<PagedResult<TransactionSummaryDto>>;
