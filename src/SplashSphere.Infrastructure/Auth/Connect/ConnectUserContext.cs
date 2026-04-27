using Microsoft.AspNetCore.Http;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.Infrastructure.Auth.Connect;

/// <summary>
/// Reads <c>sub</c> and <c>phone</c> claims from the current request when it was
/// authenticated with the <c>ConnectJwt</c> scheme. Returns empty strings and
/// <c>IsAuthenticated = false</c> for any other scheme (including Clerk) so that
/// admin requests do not accidentally appear to be Connect requests.
/// </summary>
public sealed class ConnectUserContext(IHttpContextAccessor accessor) : IConnectUserContext
{
    private readonly HttpContext? _ctx = accessor.HttpContext;

    public string ConnectUserId => _ctx?.User.FindFirst("sub")?.Value ?? string.Empty;

    public string Phone => _ctx?.User.FindFirst("phone")?.Value ?? string.Empty;

    public bool IsAuthenticated
    {
        get
        {
            if (_ctx?.User.Identity?.IsAuthenticated != true) return false;

            // Identity.AuthenticationType is the scheme that issued the ticket.
            var scheme = _ctx.User.Identity.AuthenticationType;
            return string.Equals(scheme, ConnectJwtSetup.SchemeName, StringComparison.Ordinal)
                && !string.IsNullOrEmpty(ConnectUserId);
        }
    }
}
