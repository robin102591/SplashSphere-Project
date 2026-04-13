using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetSupplyCategories;

public sealed record GetSupplyCategoriesQuery : IQuery<IReadOnlyList<SupplyCategoryDto>>;
