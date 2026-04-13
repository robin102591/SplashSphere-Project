using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetSuppliers;

public sealed record GetSuppliersQuery : IQuery<IReadOnlyList<SupplierDto>>;
