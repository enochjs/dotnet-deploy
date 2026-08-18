using IntegrationTests.Support;
using StackExchange.Redis;

namespace IntegrationTests;

public sealed class RedisIntegrationTests : IClassFixture<TestEnvironmentFixture>
{
    private readonly TestEnvironmentFixture _fixture;

    public RedisIntegrationTests(TestEnvironmentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Redis_can_set_and_get_value()
    {
        await using var connection = await ConnectionMultiplexer.ConnectAsync(_fixture.RedisConnectionString);
        var database = connection.GetDatabase();

        await database.StringSetAsync("day04:redis:ping", "pong");
        var value = await database.StringGetAsync("day04:redis:ping");

        Assert.Equal("pong", value);
    }
}