using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Commands.UpsertServiceCommission;

/// <summary>
/// Replaces the entire commission matrix for a service in one atomic operation.
/// ServiceId comes from the route; Rows come from the request body.
/// An empty Rows list clears the matrix (₱0 commission for all lookups).
/// </summary>
public sealed record UpsertServiceCommissionCommand(
    string ServiceId,
    IReadOnlyList<ServiceCommissionRowRequest> Rows) : ICommand;

/// <summary>
/// One cell in the commission matrix PUT body.
/// Field requirements by Type:
/// <list type="bullet">
///   <item>Percentage — PercentageRate set, FixedAmount null.</item>
///   <item>FixedAmount — FixedAmount set, PercentageRate null.</item>
///   <item>Hybrid      — both set.</item>
/// </list>
/// </summary>
public sealed record ServiceCommissionRowRequest(
    string VehicleTypeId,
    string SizeId,
    CommissionType Type,
    decimal? FixedAmount,
    decimal? PercentageRate);
