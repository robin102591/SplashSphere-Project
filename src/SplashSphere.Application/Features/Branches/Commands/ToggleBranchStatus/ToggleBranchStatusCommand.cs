using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Branches.Commands.ToggleBranchStatus;

/// <summary>Flips a branch's IsActive flag. Active → inactive, inactive → active.</summary>
public sealed record ToggleBranchStatusCommand(string Id) : ICommand;
