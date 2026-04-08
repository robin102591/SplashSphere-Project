using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Commands.SuspendFranchisee;

public sealed record SuspendFranchiseeCommand(string FranchiseeTenantId) : ICommand;
