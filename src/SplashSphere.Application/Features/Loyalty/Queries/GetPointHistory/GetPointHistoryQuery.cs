using MediatR;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetPointHistory;

public sealed record GetPointHistoryQuery(
    string MembershipCardId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<PointTransactionDto>>;
