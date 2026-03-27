using MediatR;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.CashAdvances.Queries.GetCashAdvances;

public sealed record GetCashAdvancesQuery(
    int Page = 1,
    int PageSize = 20,
    string? EmployeeId = null,
    CashAdvanceStatus? Status = null) : IRequest<PagedResult<CashAdvanceDto>>;
