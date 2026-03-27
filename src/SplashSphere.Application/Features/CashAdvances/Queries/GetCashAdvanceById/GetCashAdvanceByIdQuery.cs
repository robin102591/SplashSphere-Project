using MediatR;

namespace SplashSphere.Application.Features.CashAdvances.Queries.GetCashAdvanceById;

public sealed record GetCashAdvanceByIdQuery(string Id) : IRequest<CashAdvanceDto?>;
