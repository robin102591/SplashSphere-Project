namespace SplashSphere.API.Tests.TestFixtures;

/// <summary>
/// xUnit collection definition that shares a single <see cref="TestWebApplicationFactory"/>
/// across all integration test classes. This avoids spinning up multiple PostgreSQL
/// containers — one container per test run.
/// </summary>
[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Integration";
}

/// <summary>
/// Wraps <see cref="TestWebApplicationFactory"/> and seeds test data once.
/// Used as the collection fixture for all integration tests.
/// </summary>
public sealed class IntegrationTestFixture : IAsyncLifetime
{
    public TestWebApplicationFactory Factory { get; } = new();

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
        await TestDataBuilder.SeedAsync(Factory.Services);
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }
}
