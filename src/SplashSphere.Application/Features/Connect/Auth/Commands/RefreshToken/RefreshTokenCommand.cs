using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Auth.Commands.RefreshToken;

/// <summary>
/// Exchange a valid, unexpired, un-revoked refresh token for a new access+refresh pair.
/// Rotates the refresh token on every call (old row is marked revoked).
/// </summary>
public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResponse>;

public sealed record RefreshTokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);
