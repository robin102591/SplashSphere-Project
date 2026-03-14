using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Queries.GetEmployeeCommissions;

/// <summary>
/// Paginated commission history for a single employee.
/// Each row is one TransactionEmployee summary — the total commission earned
/// by this employee across all services/packages in that transaction.
/// </summary>
public sealed record GetEmployeeCommissionsQuery(
    string EmployeeId,
    int Page = 1,
    int PageSize = 20,
    DateOnly? From = null,
    DateOnly? To = null) : IQuery<PagedResult<EmployeeCommissionDto>>;

public sealed record EmployeeCommissionDto(
    string TransactionId,
    string TransactionNumber,
    DateOnly TransactionDate,
    string BranchName,
    decimal TotalCommission);
