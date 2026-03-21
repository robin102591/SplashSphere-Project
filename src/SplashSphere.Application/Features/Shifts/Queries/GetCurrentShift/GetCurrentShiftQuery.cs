using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Shifts.Queries.GetCurrentShift;

/// <summary>Returns the current open shift for the authenticated cashier at the given branch, or null if none.</summary>
public sealed record GetCurrentShiftQuery(string BranchId) : IQuery<ShiftDetailDto?>;
