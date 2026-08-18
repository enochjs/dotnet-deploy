using Infrastructure.Persistence;
using IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public sealed class DatabaseIntegrationTests : IClassFixture<TestEnvironmentFixture>
{
    private readonly TestEnvironmentFixture _fixture;

    public DatabaseIntegrationTests(TestEnvironmentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Database_can_connect_after_migrations_are_applied()
    {
        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MetaServerDbContext>();

        var canConnect = await dbContext.Database.CanConnectAsync();

        Assert.True(canConnect);
    }

    [Fact]
    public async Task Seed_data_contains_core_objects_for_later_modules()
    {
        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MetaServerDbContext>();

        var userCount = await dbContext.Users.CountAsync();
        var application = await dbContext.Applications
            .Include(item => item.SubApplications)
            .SingleAsync(item => item.AppKey == "meta-web");
        var requirement = await dbContext.Requirements
            .Include(item => item.Developers)
            .Include(item => item.Followers)
            .SingleAsync(item => item.Name == "Seed requirement");
        var iteration = await dbContext.Iterations
            .Include(item => item.Requirements)
            .SingleAsync(item => item.Branch == "feature/seed-iteration-001");
        var template = await dbContext.PipelineTemplates
            .Include(item => item.Stages)
            .ThenInclude(item => item.Jobs)
            .SingleAsync(item => item.TemplateKey == "frontend-default");

        Assert.Equal(2, userCount);
        Assert.Single(application.SubApplications);
        Assert.Single(requirement.Developers);
        Assert.Single(requirement.Followers);
        Assert.Single(iteration.Requirements);
        Assert.Equal(2, template.Stages.Count);
        Assert.Equal(3, template.Stages.SelectMany(item => item.Jobs).Count());
    }
}