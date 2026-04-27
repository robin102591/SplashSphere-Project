using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Auth.Commands.SignOut;

/// <summary>
/// Revoke a refresh token (client-initiated sign-out). Idempotent — accepts unknown or
/// already-revoked tokens without error so clients can always "log out" safely.
/// </summary>
public sealed record SignOutCommand(string RefreshToken) : ICommand;
