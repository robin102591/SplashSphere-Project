using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Profile.Queries.GetMyProfile;

/// <summary>
/// Read the authenticated Connect user's profile with their vehicles.
/// Returns null if the caller is unauthenticated or the user no longer exists.
/// </summary>
public sealed record GetMyProfileQuery : IQuery<ConnectProfileDto?>;
