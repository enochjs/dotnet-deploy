using Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace UnitTests;

public class OptionsBindingTests
{
    [Fact]
    public void PostgresOptions_BindsFromConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Postgres:ConnectionString"] = "Host=localhost;Database=test",
                ["Redis:ConnectionString"] = "localhost:6379",
                ["Git:RepositoryRoot"] = "/tmp/repositories",
                ["Git:DefaultBranch"] = "main",
                ["DingTalk:BaseUrl"] = "https://api.dingtalk.com",
                ["OSS:Endpoint"] = "https://oss-cn-hangzhou.aliyuncs.com",
                ["OSS:BucketName"] = "bucket",
                ["InnerServer:BaseUrl"] = "http://localhost:5000",
                ["InnerServer:TimeoutSeconds"] = "30",
                ["Monitor:Enabled"] = "true",
                ["Monitor:SlowRequestThresholdMs"] = "1000",
                ["Logger:IncludeRequestBody"] = "false",
                ["Logger:IncludeResponseBody"] = "false"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddApplicationOptions();

        using var provider = services.BuildServiceProvider(validateScopes: true);
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>().Value;

        Assert.Equal("Host=localhost;Database=test", options.ConnectionString);
    }

    [Fact]
    public void MissingRequiredOptions_FailsValidation()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Redis:ConnectionString"] = "localhost:6379"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddApplicationOptions();

        using var provider = services.BuildServiceProvider(validateScopes: true);

        Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<PostgresOptions>>().Value);
    }
}