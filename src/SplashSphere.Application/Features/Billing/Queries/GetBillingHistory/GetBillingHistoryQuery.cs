using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Billing.Queries.GetBillingHistory;

public sealed record GetBillingHistoryQuery(
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<BillingRecordDto>>;
