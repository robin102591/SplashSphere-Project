namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Scoped per-request identity for the Customer Connect app. Populated from the
/// <c>ConnectJwt</c> bearer token's <c>sub</c> and <c>phone</c> claims.
/// <para>
/// <see cref="IsAuthenticated"/> is true only when the request actually carries a
/// valid Connect token — handlers that need the current customer should check this
/// first and return 401 otherwise.
/// </para>
/// </summary>
public interface IConnectUserContext
{
    string ConnectUserId { get; }
    string Phone { get; }
    bool IsAuthenticated { get; }
}
