using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.UpdateServiceSupplyUsage;

public sealed record UsageEntry(string SupplyItemId, string? SizeId, decimal QuantityPerUse);

public sealed record UpdateServiceSupplyUsageCommand(
    string ServiceId,
    IReadOnlyList<UsageEntry> Usages) : ICommand;
