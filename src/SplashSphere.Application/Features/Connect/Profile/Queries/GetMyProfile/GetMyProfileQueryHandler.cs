using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Profile.Queries.GetMyProfile;

public sealed class GetMyProfileQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<GetMyProfileQuery, ConnectProfileDto?>
{
    public async Task<ConnectProfileDto?> Handle(
        GetMyProfileQuery request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated) return null;

        var userId = connectUser.ConnectUserId;

        var user = await db.ConnectUsers
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Phone,
                u.Name,
                u.Email,
                u.AvatarUrl,
                u.CreatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) return null;

        var vehicles = await db.ConnectVehicles
            .AsNoTracking()
            .Where(v => v.ConnectUserId == userId)
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

        return new ConnectProfileDto(
            user.Id,
            user.Phone,
            user.Name,
            user.Email,
            user.AvatarUrl,
            user.CreatedAt,
            vehicles);
    }
}
