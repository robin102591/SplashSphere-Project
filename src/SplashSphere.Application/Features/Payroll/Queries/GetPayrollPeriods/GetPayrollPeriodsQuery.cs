using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Queries.GetPayrollPeriods;

/// <summary>Returns a paginated list of payroll periods for the current tenant.</summary>
public sealed record GetPayrollPeriodsQuery(
    int Page = 1,
    int PageSize = 20,
    PayrollStatus? Status = null,
    int? Year = null) : IQuery<PagedResult<PayrollPeriodSummaryDto>>;
