using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace SplashSphere.Infrastructure.Authentication;

/// <summary>
/// Registers a second <see cref="JwtBearerDefaults.AuthenticationScheme"/> named
/// <see cref="SchemeName"/> for the Customer Connect app. Uses a symmetric
/// signing key configured under <c>Jwt:Connect:SigningKey</c> and validates against
/// the Connect-specific issuer and audience so Clerk tokens and Connect tokens are
/// strictly separated.
/// <para>
/// Consumers protect Connect endpoints with <c>[Authorize(AuthenticationSchemes = "ConnectJwt")]</c>.
/// Admin/POS endpoints keep their default Bearer (Clerk) scheme.
/// </para>
/// </summary>
public static class ConnectJwtSetup
{
    public const string SchemeName = "ConnectJwt";

    public static AuthenticationBuilder AddConnectJwtAuthentication(
        this AuthenticationBuilder builder,
        IConfiguration configuration)
    {
        var signingKey = configuration["Jwt:Connect:SigningKey"];
        var issuer = configuration["Jwt:Connect:Issuer"] ?? "splashsphere-api";
        var audience = configuration["Jwt:Connect:Audience"] ?? "splashsphere.connect";

        return builder.AddJwtBearer(SchemeName, options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = string.IsNullOrEmpty(signingKey)
                    ? null
                    : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                NameClaimType = "sub",
                // Stamp the identity's AuthenticationType with our scheme name so
                // ConnectUserContext.IsAuthenticated can distinguish Connect-issued
                // tickets from Clerk tickets. Without this the default is
                // "AuthenticationTypes.Federation" and the check fails.
                AuthenticationType = SchemeName,
            };
        });
    }
}
