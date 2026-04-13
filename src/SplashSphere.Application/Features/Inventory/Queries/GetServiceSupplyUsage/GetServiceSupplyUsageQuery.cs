using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetServiceSupplyUsage;

public sealed record GetServiceSupplyUsageQuery(string ServiceId) : IQuery<IReadOnlyList<ServiceSupplyUsageDto>>;
