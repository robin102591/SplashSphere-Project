using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetServiceCostBreakdown;

public sealed record GetServiceCostBreakdownQuery(string ServiceId) : IQuery<ServiceCostBreakdownDto?>;
