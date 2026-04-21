namespace SplashSphere.Domain.Entities;

/// <summary>
/// Hashed refresh token issued to a <see cref="ConnectUser"/> after successful OTP sign-in.
/// <para>
/// <b>Storage:</b> only the SHA-256 hash of the token is stored — the plaintext token
/// is returned to the client once and never persisted. On use, the Connect token
/// service rotates the token: a new row is inserted and the old row's
/// <see cref="RevokedAt"/> is set.
/// </para>
/// <para>
/// Not tenant-scoped — refresh tokens belong to the global <see cref="ConnectUser"/>.
/// </para>
/// </summary>
public sealed class ConnectRefreshToken
{
    private ConnectRefreshToken() { } // EF Core

    public ConnectRefreshToken(string connectUserId, string tokenHash, DateTime expiresAt)
    {
        Id = Guid.NewGuid().ToString();
        ConnectUserId = connectUserId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public string Id { get; set; } = string.Empty;
    public string ConnectUserId { get; set; } = string.Empty;

    /// <summary>SHA-256 hex digest of the refresh token — never the raw value.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    /// <summary>Set when the token is rotated on use or explicitly revoked.</summary>
    public DateTime? RevokedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public ConnectUser ConnectUser { get; set; } = null!;
}
