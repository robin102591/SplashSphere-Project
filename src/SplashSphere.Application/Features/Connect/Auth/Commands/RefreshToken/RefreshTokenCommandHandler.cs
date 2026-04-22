using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(IConnectTokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var pair = await tokenService.RefreshAsync(request.RefreshToken, cancellationToken);
        if (pair is null)
        {
            return Result.Failure<RefreshTokenResponse>(
                Error.Unauthorized("Refresh token is invalid, expired, or revoked."));
        }

        return Result.Success(new RefreshTokenResponse(
            pair.AccessToken,
            pair.AccessTokenExpiresAt,
            pair.RefreshToken,
            pair.RefreshTokenExpiresAt));
    }
}
