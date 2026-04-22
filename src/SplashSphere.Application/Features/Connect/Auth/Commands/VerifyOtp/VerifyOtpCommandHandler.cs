using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Auth.Commands.VerifyOtp;

public sealed class VerifyOtpCommandHandler(
    IApplicationDbContext db,
    IOtpStore otpStore,
    IConnectTokenService tokenService)
    : IRequestHandler<VerifyOtpCommand, Result<VerifyOtpResponse>>
{
    public async Task<Result<VerifyOtpResponse>> Handle(
        VerifyOtpCommand request,
        CancellationToken cancellationToken)
    {
        var phone = PhoneNumber.TryNormalize(request.PhoneNumber);
        if (phone is null)
        {
            return Result.Failure<VerifyOtpResponse>(
                Error.Validation("Phone number must be a valid Philippine mobile number."));
        }

        var stored = await otpStore.GetCodeAsync(phone, cancellationToken);
        if (stored is null || !string.Equals(stored, request.Code, StringComparison.Ordinal))
        {
            return Result.Failure<VerifyOtpResponse>(
                new Error("OTP_INVALID", "The code is invalid or has expired."));
        }

        // Consume the code immediately so it cannot be reused.
        await otpStore.DeleteCodeAsync(phone, cancellationToken);

        // Find or create the ConnectUser (phone is globally unique).
        var user = await db.ConnectUsers
            .FirstOrDefaultAsync(u => u.Phone == phone, cancellationToken);

        var isNew = user is null;
        if (user is null)
        {
            user = new ConnectUser(phone, name: string.Empty);
            db.ConnectUsers.Add(user);
            await db.SaveChangesAsync(cancellationToken);
        }

        var pair = await tokenService.IssuePairAsync(user.Id, user.Phone, cancellationToken);

        return Result.Success(new VerifyOtpResponse(
            pair.AccessToken,
            pair.AccessTokenExpiresAt,
            pair.RefreshToken,
            pair.RefreshTokenExpiresAt,
            new ConnectUserDto(user.Id, user.Phone, user.Name, user.Email, user.AvatarUrl, isNew)));
    }
}
