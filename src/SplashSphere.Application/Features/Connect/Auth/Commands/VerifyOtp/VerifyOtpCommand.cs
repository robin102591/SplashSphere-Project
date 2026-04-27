using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Auth.Commands.VerifyOtp;

/// <summary>
/// Consume an OTP code and issue a Connect JWT pair.
/// Creates a new <c>ConnectUser</c> on first successful verification for a phone.
/// </summary>
public sealed record VerifyOtpCommand(string PhoneNumber, string Code) : ICommand<VerifyOtpResponse>;

public sealed record VerifyOtpResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    ConnectUserDto User);

public sealed record ConnectUserDto(
    string Id,
    string Phone,
    string Name,
    string? Email,
    string? AvatarUrl,
    bool IsNew);
