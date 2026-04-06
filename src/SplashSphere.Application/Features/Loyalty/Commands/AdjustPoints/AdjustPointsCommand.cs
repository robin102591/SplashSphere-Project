using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Loyalty.Commands.AdjustPoints;

public sealed record AdjustPointsCommand(
    string MembershipCardId,
    int Points,
    string Reason) : ICommand;
