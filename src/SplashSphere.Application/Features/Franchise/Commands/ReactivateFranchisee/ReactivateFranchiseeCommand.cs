using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Commands.ReactivateFranchisee;

public sealed record ReactivateFranchiseeCommand(string FranchiseeTenantId) : ICommand;
