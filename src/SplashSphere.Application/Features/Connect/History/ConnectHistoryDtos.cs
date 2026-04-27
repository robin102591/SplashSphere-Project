namespace SplashSphere.Application.Features.Connect.History;

/// <summary>
/// One row in the Connect customer's cross-tenant service history —
/// i.e. a completed transaction at any car wash they've joined.
/// </summary>
public sealed record ConnectServiceHistoryItemDto(
    string TransactionId,
    string TransactionNumber,
    string TenantId,
    string TenantName,
    string BranchId,
    string BranchName,
    string PlateNumber,
    decimal FinalAmount,
    int PointsEarned,
    DateTime CompletedAt,
    IReadOnlyList<string> ServiceNames);
