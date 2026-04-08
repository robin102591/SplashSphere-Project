using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Commands.InviteFranchisee;

public sealed record InviteFranchiseeCommand(
    string Email,
    string BusinessName,
    string? OwnerName,
    string? FranchiseCode,
    string? TerritoryName) : ICommand<string>;
