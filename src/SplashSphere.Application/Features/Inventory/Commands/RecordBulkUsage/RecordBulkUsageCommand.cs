using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.RecordBulkUsage;

public sealed record BulkUsageItem(string SupplyItemId, decimal Quantity, string? Notes = null);

public sealed record RecordBulkUsageCommand(
    string BranchId,
    IReadOnlyList<BulkUsageItem> Items) : ICommand<int>;
