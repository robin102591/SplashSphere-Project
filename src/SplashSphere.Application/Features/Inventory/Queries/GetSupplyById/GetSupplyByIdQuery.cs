using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetSupplyById;

public sealed record GetSupplyByIdQuery(string Id) : IQuery<SupplyItemDetailDto?>;
