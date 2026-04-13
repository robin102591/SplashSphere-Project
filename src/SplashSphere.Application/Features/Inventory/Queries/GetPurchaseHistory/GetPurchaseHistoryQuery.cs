using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetPurchaseHistory;

public sealed record GetPurchaseHistoryQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null) : IQuery<PurchaseHistoryDto>;
