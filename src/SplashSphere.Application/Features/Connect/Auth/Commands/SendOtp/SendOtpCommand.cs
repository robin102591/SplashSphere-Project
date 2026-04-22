using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Auth.Commands.SendOtp;

/// <summary>
/// Request an OTP code for <paramref name="PhoneNumber"/>.
/// Rate-limited by <see cref="IOtpStore"/> (60s cooldown, 5/day cap per phone).
/// Returns the server-side TTL so the client can show an accurate countdown.
/// </summary>
public sealed record SendOtpCommand(string PhoneNumber) : ICommand<SendOtpResponse>;

public sealed record SendOtpResponse(int TtlSeconds);
