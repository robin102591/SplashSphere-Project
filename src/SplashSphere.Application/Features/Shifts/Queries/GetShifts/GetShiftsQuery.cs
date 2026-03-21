using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Queries.GetShifts;

public sealed record GetShiftsQuery(
    string? BranchId,
    string? CashierId,
    DateOnly? DateFrom,
    DateOnly? DateTo,
    ShiftStatus? Status,
    ReviewStatus? ReviewStatus,
    int Page,
    int PageSize) : IQuery<PagedResult<ShiftSummaryDto>>;
