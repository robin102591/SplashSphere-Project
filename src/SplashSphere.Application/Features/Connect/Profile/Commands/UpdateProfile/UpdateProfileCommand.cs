using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Profile.Commands.UpdateProfile;

/// <summary>
/// Update the authenticated Connect user's display fields. Phone cannot be changed
/// here — that would invalidate the global identity.
/// </summary>
public sealed record UpdateProfileCommand(
    string Name,
    string? Email,
    string? AvatarUrl) : ICommand<ConnectProfileDto>;
