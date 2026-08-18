using Infrastructure.Persistence;
using Infrastructure.Persistence.SeedData;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace IntegrationTests.Support;

public sealed class TestEnvironmentFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("dotnet_meta_server_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public string PostgresConnectionString => _postgres.GetConnectionString();

    public string RedisConnectionString => _redis.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    var overrides = new Dictionary<string, string?>
                    {
                        ["Postgres:ConnectionString"] = PostgresConnectionString,
                        ["Redis:ConnectionString"] = RedisConnectionString,
                    };

                    configuration.AddInMemoryCollection(overrides);
                });
            });

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MetaServerDbContext>();
        await dbContext.Database.MigrateAsync();
        await DevelopmentSeedData.SeedAsync(dbContext);
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _redis.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}