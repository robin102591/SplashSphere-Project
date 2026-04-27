using Microsoft.AspNetCore.Authorization;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints.Connect;

/// <summary>
/// Shared builder helpers for <c>/api/v1/connect/*</c> route groups.
/// Centralizes the authorization policy so every Connect module uses the
/// <see cref="ConnectJwtSetup.SchemeName"/> scheme instead of the default Clerk one.
/// </summary>
internal static class ConnectRouteGroupExtensions
{
    /// <summary>
    /// Build a route group under <paramref name="prefix"/> that requires a valid
    /// Connect JWT. Tagged with <paramref name="tag"/> for OpenAPI grouping.
    /// </summary>
    public static RouteGroupBuilder MapConnectGroup(
        this IEndpointRouteBuilder app,
        string prefix,
        string tag)
    {
        return app.MapGroup(prefix)
            .RequireAuthorization(new AuthorizeAttribute
            {
                AuthenticationSchemes = ConnectJwtSetup.SchemeName,
            })
            .WithTags(tag);
    }
}
