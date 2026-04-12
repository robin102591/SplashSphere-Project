using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SplashSphere.Infrastructure.Auth;
using SplashSphere.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SplashSphere.API.Tests.TestFixtures;

/// <summary>
/// Custom <see cref="WebApplicationFactory{TEntryPoint}"/> that uses Testcontainers
/// to spin up a real PostgreSQL instance, replaces authentication with
/// <see cref="TestAuthHandler"/>, and removes Hangfire registrations.
/// Implements <see cref="IAsyncLifetime"/> for container lifecycle management.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("splashsphere_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Apply EF Core migrations against the Testcontainers database.
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // ── Replace ApplicationDbContext with Testcontainers connection string ──
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseNpgsql(_postgres.GetConnectionString());
            });

            // ── Replace authentication with TestAuthHandler ──────────────────────
            services.RemoveAll<AuthenticationSchemeOptions>();

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

            // ── Remove Hangfire services (no background job server in tests) ─────
            services.RemoveAll<IGlobalConfiguration>();
            services.RemoveAll<IBackgroundJobClient>();
            services.RemoveAll<IRecurringJobManager>();

            // Remove all Hangfire-related hosted services
            services.RemoveAll(typeof(IHostedService));
        });
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> pre-configured to authenticate
    /// as the specified tenant/user. Headers are sent with every request.
    /// </summary>
    public HttpClient CreateClientForTenant(
        string tenantId,
        string? userId = null,
        string? role = null)
    {
        var client = CreateClient();

        client.DefaultRequestHeaders.Add(TestAuthHandler.TenantIdHeader, tenantId);

        if (userId is not null)
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId);

        if (role is not null)
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);

        return client;
    }
}
