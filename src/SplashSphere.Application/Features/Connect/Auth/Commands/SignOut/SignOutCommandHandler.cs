using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Auth.Commands.SignOut;

public sealed class SignOutCommandHandler(IConnectTokenService tokenService)
    : IRequestHandler<SignOutCommand, Result>
{
    public async Task<Result> Handle(SignOutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            await tokenService.RevokeAsync(request.RefreshToken, cancellationToken);
        }

        return Result.Success();
    }
}
