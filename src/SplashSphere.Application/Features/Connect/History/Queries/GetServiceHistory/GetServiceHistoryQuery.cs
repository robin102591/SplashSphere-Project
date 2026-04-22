using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.History.Queries.GetServiceHistory;

/// <summary>
/// List the authenticated customer's completed services across every tenant
/// they've joined, newest first. <paramref name="Take"/> defaults to 50
/// (max 200) inside the handler.
/// </summary>
public sealed record GetServiceHistoryQuery(int? Take)
    : IQuery<IReadOnlyList<ConnectServiceHistoryItemDto>>;
