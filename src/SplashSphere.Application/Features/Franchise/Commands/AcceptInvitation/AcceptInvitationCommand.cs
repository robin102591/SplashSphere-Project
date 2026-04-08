using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Commands.AcceptInvitation;

public sealed record AcceptInvitationCommand(
    string Token,
    string BusinessName,
    string Email,
    string ContactNumber,
    string Address,
    string BranchName,
    string BranchCode,
    string BranchAddress,
    string BranchContactNumber) : ICommand<string>;
