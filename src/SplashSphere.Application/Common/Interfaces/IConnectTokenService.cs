namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Issues, validates, and rotates JWT access + refresh tokens for the Customer
/// Connect app. Uses a <b>separate signing key and audience</b> from the admin/POS
/// Clerk tokens so the two surfaces cannot accidentally accept each other's tokens.
/// <para>
/// Signing parameters live under <c>Jwt:Connect:*</c> in configuration:
/// <c>SigningKey</c>, <c>Issuer</c>, <c>Audience</c>, <c>AccessTokenMinutes</c>, <c>RefreshTokenDays</c>.
/// </para>
/// <para>
/// Refresh tokens are rotated on every use: the old row is revoked and a new one
/// is inserted. Only the SHA-256 hash is persisted — the plaintext exists only in
/// the response body.
/// </para>
/// </summary>
public interface IConnectTokenService
{
    /// <summary>
    /// Issue a fresh access/refresh pair for <paramref name="connectUserId"/>.
    /// Persists the refresh token's hash and returns the plaintext tokens once.
    /// </summary>
    Task<ConnectTokenPair> IssuePairAsync(string connectUserId, string phone, CancellationToken ct = default);

    /// <summary>
    /// Consume a refresh token: validate it, revoke the old row, and return a
    /// new access/refresh pair. Returns null if the token is missing, expired,
    /// already revoked, or does not belong to a known user.
    /// </summary>
    Task<ConnectTokenPair?> RefreshAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>Revoke a refresh token (sign-out). Idempotent.</summary>
    Task RevokeAsync(string refreshToken, CancellationToken ct = default);
}

/// <summary>A freshly issued access/refresh token pair. Both values are plaintext.</summary>
public sealed record ConnectTokenPair(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);
