using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Auth.Connect;

/// <summary>
/// Issues HS256 JWTs for the Connect app. Access tokens are short-lived (30 min default);
/// refresh tokens are 256-bit random strings with SHA-256 hashes persisted in
/// <see cref="IApplicationDbContext.ConnectRefreshTokens"/>. Rotation on every refresh.
/// </summary>
public sealed class ConnectTokenService(
    IApplicationDbContext db,
    IConfiguration configuration) : IConnectTokenService
{
    private readonly string _signingKey = configuration["Jwt:Connect:SigningKey"]
        ?? throw new InvalidOperationException("Jwt:Connect:SigningKey is not configured.");
    private readonly string _issuer = configuration["Jwt:Connect:Issuer"] ?? "splashsphere-api";
    private readonly string _audience = configuration["Jwt:Connect:Audience"] ?? "splashsphere.connect";
    private readonly int _accessMinutes = int.TryParse(configuration["Jwt:Connect:AccessTokenMinutes"], out var a) ? a : 30;
    private readonly int _refreshDays = int.TryParse(configuration["Jwt:Connect:RefreshTokenDays"], out var r) ? r : 30;

    public async Task<ConnectTokenPair> IssuePairAsync(string connectUserId, string phone, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var accessExp = now.AddMinutes(_accessMinutes);
        var refreshExp = now.AddDays(_refreshDays);

        var access = CreateAccessToken(connectUserId, phone, now, accessExp);
        var (refresh, refreshHash) = CreateRefreshToken();

        db.ConnectRefreshTokens.Add(new ConnectRefreshToken(connectUserId, refreshHash, refreshExp));
        await db.SaveChangesAsync(ct);

        return new ConnectTokenPair(access, accessExp, refresh, refreshExp);
    }

    public async Task<ConnectTokenPair?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = Sha256Hex(refreshToken);
        var row = await db.ConnectRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (row is null || row.RevokedAt is not null || row.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        // Load user for phone claim
        var user = await db.ConnectUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == row.ConnectUserId, ct);
        if (user is null)
        {
            return null;
        }

        // Rotate: revoke old, issue new
        row.RevokedAt = DateTime.UtcNow;
        return await IssuePairAsync(user.Id, user.Phone, ct);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = Sha256Hex(refreshToken);
        var row = await db.ConnectRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (row is null || row.RevokedAt is not null)
        {
            return;
        }

        row.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private string CreateAccessToken(string connectUserId, string phone, DateTime issuedAt, DateTime expiresAt)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_signingKey);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, connectUserId),
            new("phone", phone),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var jwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private static (string plaintext, string hash) CreateRefreshToken()
    {
        Span<byte> buffer = stackalloc byte[32];
        RandomNumberGenerator.Fill(buffer);
        var plaintext = Convert.ToHexString(buffer).ToLowerInvariant();
        return (plaintext, Sha256Hex(plaintext));
    }

    private static string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
