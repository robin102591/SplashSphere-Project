using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Commands.CalculateRoyalties;

public sealed record CalculateRoyaltiesCommand(
    string FranchiseeTenantId,
    DateTime PeriodStart,
    DateTime PeriodEnd) : ICommand<string>;
