using MediatR;

namespace SplashSphere.Application.Features.CashAdvances.Queries.GetEmployeeCashAdvances;

public sealed record GetEmployeeCashAdvancesQuery(string EmployeeId) : IRequest<IReadOnlyList<CashAdvanceDto>>;
