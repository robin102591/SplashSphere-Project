using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Loyalty.Queries.GetPointsHistory;

/// <summary>
/// List the authenticated customer's point movements at a single tenant,
/// newest first. <paramref name="Take"/> defaults to 50 on the handler.
/// Returns an empty list when the caller is not linked or not enrolled.
/// </summary>
public sealed record GetPointsHistoryQuery(string TenantId, int? Take)
    : IQuery<IReadOnlyList<ConnectPointTransactionDto>>;
