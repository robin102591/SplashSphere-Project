using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Auth.Commands.VerifyPin;

public sealed class VerifyPinCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<VerifyPinCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        VerifyPinCommand request,
        CancellationToken cancellationToken)
    {
        var user = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == tenantContext.UserId)
            .Select(u => new { u.PinHash })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return Result.Failure<bool>(Error.NotFound("User", tenantContext.UserId));

        if (user.PinHash is null)
            return Result.Failure<bool>(Error.Validation("No PIN has been configured. Contact your administrator."));

        var isValid = BCrypt.Net.BCrypt.Verify(request.Pin, user.PinHash);
        return Result.Success(isValid);
    }
}
