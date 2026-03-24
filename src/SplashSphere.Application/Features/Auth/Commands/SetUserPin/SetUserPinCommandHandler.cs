using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Auth.Commands.SetUserPin;

public sealed class SetUserPinCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<SetUserPinCommand, Result>
{
    public async Task<Result> Handle(
        SetUserPinCommand request,
        CancellationToken cancellationToken)
    {
        // Admin-only: only org:admin can set PINs
        if (tenantContext.Role is null || !tenantContext.Role.Contains("admin"))
            return Result.Failure(Error.Forbidden("Only administrators can set user PINs."));

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(Error.NotFound("User", request.UserId));

        user.PinHash = BCrypt.Net.BCrypt.HashPassword(request.Pin);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
