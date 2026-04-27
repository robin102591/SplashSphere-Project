using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Profile.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<UpdateProfileCommand, Result<ConnectProfileDto>>
{
    public async Task<Result<ConnectProfileDto>> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated)
        {
            return Result.Failure<ConnectProfileDto>(Error.Unauthorized("Sign in required."));
        }

        var user = await db.ConnectUsers
            .FirstOrDefaultAsync(u => u.Id == connectUser.ConnectUserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<ConnectProfileDto>(Error.NotFound("ConnectUser", connectUser.ConnectUserId));
        }

        user.Name = request.Name.Trim();
        user.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();

        // UoWBehavior saves; load vehicles for the response.
        var vehicles = await db.ConnectVehicles
            .AsNoTracking()
            .Where(v => v.ConnectUserId == user.Id)
            .OrderBy(v => v.CreatedAt)
            .Select(v => new ConnectVehicleDto(
                v.Id,
                v.MakeId,
                v.Make.Name,
                v.ModelId,
                v.Model.Name,
                v.PlateNumber,
                v.Color,
                v.Year))
            .ToListAsync(cancellationToken);

        return Result.Success(new ConnectProfileDto(
            user.Id,
            user.Phone,
            user.Name,
            user.Email,
            user.AvatarUrl,
            user.CreatedAt,
            vehicles));
    }
}
