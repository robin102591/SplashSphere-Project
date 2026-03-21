using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.OpenShift;

/// <summary>Opens a new cashier shift. Returns the new shift ID.</summary>
public sealed record OpenShiftCommand(
    string BranchId,
    decimal OpeningCashFund) : ICommand<string>;
