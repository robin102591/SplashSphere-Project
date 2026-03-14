using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Branches.Commands.CreateBranch;

/// <summary>Creates a new branch for the current tenant. Returns the new branch ID.</summary>
public sealed record CreateBranchCommand(
    string Name,
    string Code,
    string Address,
    string ContactNumber) : ICommand<string>;
