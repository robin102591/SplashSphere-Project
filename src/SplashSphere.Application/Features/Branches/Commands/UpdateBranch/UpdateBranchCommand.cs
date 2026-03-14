using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Branches.Commands.UpdateBranch;

/// <summary>Updates mutable fields on an existing branch. Id comes from the route.</summary>
public sealed record UpdateBranchCommand(
    string Id,
    string Name,
    string Code,
    string Address,
    string ContactNumber) : ICommand;
