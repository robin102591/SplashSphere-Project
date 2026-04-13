using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Queries.GetEquipment;

public sealed record GetEquipmentQuery(
    string? BranchId = null,
    EquipmentStatus? Status = null,
    int Page = 1,
    int PageSize = 50) : IQuery<PagedResult<EquipmentDto>>;
