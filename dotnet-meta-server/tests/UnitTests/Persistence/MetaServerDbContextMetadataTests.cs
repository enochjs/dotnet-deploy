using Domain.Entities;
using Domain.Entities.Pipelines;
using Domain.Entities.Pipelines.Templates;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Persistence;

using AppEntity = Domain.Entities.Application;

public sealed class MetaServerDbContextMetadataTests
{
    private readonly MetaServerDbContext _dbContext = new(
        new DbContextOptionsBuilder<MetaServerDbContext>()
            .UseNpgsql("Host=localhost;Database=metadata_test;Username=test;Password=test")
            .Options);

    [Fact]
    public void Model_contains_day03_core_tables()
    {
        var tableNames = _dbContext.Model
            .GetEntityTypes()
            .Select(type => type.GetTableName())
            .Where(tableName => tableName is not null)
            .ToHashSet();

        string[] expectedTables =
        [
            "applications",
            "sub_applications",
            "users",
            "requirements",
            "iterations",
            "integration_releases",
            "integration_release_apps",
            "pipeline_templates",
            "pipeline_template_stages",
            "pipeline_template_jobs",
            "pipelines",
            "pipeline_jobs",
            "deploys",
            "app_monitors",
        ];

        Assert.All(expectedTables, tableName => Assert.Contains(tableName, tableNames));
    }

    [Theory]
    [InlineData(typeof(AppEntity), "applications", typeof(int))]
    [InlineData(typeof(SubApplication), "sub_applications", typeof(int))]
    [InlineData(typeof(User), "users", typeof(int))]
    [InlineData(typeof(Requirement), "requirements", typeof(int))]
    [InlineData(typeof(Iteration), "iterations", typeof(int))]
    [InlineData(typeof(IntegrationRelease), "integration_releases", typeof(int))]
    [InlineData(typeof(IntegrationReleaseApp), "integration_release_apps", typeof(int))]
    [InlineData(typeof(PipelineTemplate), "pipeline_templates", typeof(int))]
    [InlineData(typeof(PipelineTemplateStage), "pipeline_template_stages", typeof(int))]
    [InlineData(typeof(PipelineTemplateJob), "pipeline_template_jobs", typeof(int))]
    [InlineData(typeof(Pipeline), "pipelines", typeof(Guid))]
    [InlineData(typeof(PipelineJob), "pipeline_jobs", typeof(Guid))]
    [InlineData(typeof(Deploy), "deploys", typeof(Guid))]
    [InlineData(typeof(AppMonitor), "app_monitors", typeof(Guid))]
    public void Primary_keys_are_configured(Type entityType, string tableName, Type keyType)
    {
        var entity = _dbContext.Model.FindEntityType(entityType);

        Assert.NotNull(entity);
        Assert.Equal(tableName, entity.GetTableName());

        var key = Assert.Single(entity.FindPrimaryKey()!.Properties);
        Assert.Equal("id", key.GetColumnName());
        Assert.Equal(keyType, key.ClrType);
    }

    [Theory]
    [InlineData(typeof(AppEntity), "app_key")]
    [InlineData(typeof(AppEntity), "created_at")]
    [InlineData(typeof(SubApplication), "parent_application_id")]
    [InlineData(typeof(SubApplication), "upload_to_oss")]
    [InlineData(typeof(Iteration), "integration_release_id")]
    [InlineData(typeof(IntegrationReleaseApp), "sub_application_id")]
    [InlineData(typeof(Pipeline), "pipeline_template_id")]
    [InlineData(typeof(PipelineJob), "pipeline_id")]
    [InlineData(typeof(Deploy), "integration_release_version")]
    [InlineData(typeof(AppMonitor), "resolved_at")]
    public void Important_columns_use_snake_case(Type entityType, string columnName)
    {
        var entity = _dbContext.Model.FindEntityType(entityType);

        Assert.NotNull(entity);
        Assert.Contains(entity.GetProperties(), property => property.GetColumnName() == columnName);
    }

    [Theory]
    [InlineData(typeof(AppEntity), "ranchers")]
    [InlineData(typeof(SubApplication), "variables")]
    [InlineData(typeof(PipelineTemplateJob), "extra")]
    [InlineData(typeof(Pipeline), "extra")]
    [InlineData(typeof(PipelineJob), "extra")]
    public void Json_fields_use_jsonb(Type entityType, string columnName)
    {
        var entity = _dbContext.Model.FindEntityType(entityType);
        var property = entity!.GetProperties().Single(item => item.GetColumnName() == columnName);

        Assert.Equal("jsonb", property.GetColumnType());
    }

    [Theory]
    [InlineData(typeof(AppEntity), nameof(AppEntity.CreatedAt))]
    [InlineData(typeof(AppEntity), nameof(AppEntity.UpdatedAt))]
    [InlineData(typeof(Requirement), nameof(Requirement.OnlineAt))]
    [InlineData(typeof(Pipeline), nameof(Pipeline.CreatedAt))]
    [InlineData(typeof(Deploy), nameof(Deploy.CreatedAt))]
    [InlineData(typeof(AppMonitor), nameof(AppMonitor.ResolvedAt))]
    public void Time_fields_use_timestamp_with_time_zone(Type entityType, string propertyName)
    {
        var entity = _dbContext.Model.FindEntityType(entityType);
        var property = entity!.FindProperty(propertyName);

        Assert.NotNull(property);
        Assert.Equal("timestamp with time zone", property.GetColumnType());
    }

    [Fact]
    public void Important_indexes_are_configured()
    {
        AssertHasIndex<User>("ix_users_mobile", true, "mobile");
        AssertHasIndex<User>("ix_users_ding_talk_user_id", false, "ding_talk_user_id");
        AssertHasIndex<SubApplication>("ix_sub_applications_app_key", true, "app_key");
        AssertHasIndex<IntegrationRelease>("ix_integration_releases_version", true, "version");
        AssertHasIndex<PipelineTemplate>("ix_pipeline_templates_name", true, "name");
        AssertHasIndex<PipelineTemplate>("ix_pipeline_templates_template_key", true, "template_key");
        AssertHasIndex<Pipeline>("ix_pipelines_iteration_id", false, "iteration_id");
        AssertHasIndex<PipelineJob>("ix_pipeline_jobs_pipeline_stage_job", true, "pipeline_id", "stage_seq", "job_key");
        AssertHasIndex<Deploy>("ix_deploys_app_key", false, "app_key");
        AssertHasIndex<AppMonitor>("ix_app_monitors_app_key_env", false, "app_key", "env");
    }

    [Fact]
    public void Core_relationship_delete_behaviors_are_explicit()
    {
        AssertHasForeignKey<SubApplication>("parent_application_id", DeleteBehavior.Restrict);
        AssertHasForeignKey<PipelineTemplateStage>("pipeline_template_id", DeleteBehavior.Cascade);
        AssertHasForeignKey<PipelineTemplateJob>("pipeline_template_stage_id", DeleteBehavior.Cascade);
        AssertHasForeignKey<PipelineJob>("pipeline_id", DeleteBehavior.Cascade);
        AssertHasForeignKey<Deploy>("pipeline_id", DeleteBehavior.SetNull);
    }

    private void AssertHasIndex<TEntity>(
        string databaseName,
        bool unique,
        params string[] columns)
    {
        var entity = _dbContext.Model.FindEntityType(typeof(TEntity));
        Assert.NotNull(entity);

        var matchedIndex = entity
            .GetIndexes()
            .SingleOrDefault(index =>
                index.GetDatabaseName() == databaseName
                && index.IsUnique == unique
                && index.Properties.Select(property => property.GetColumnName()).SequenceEqual(columns));

        Assert.NotNull(matchedIndex);
    }

    private void AssertHasForeignKey<TEntity>(string columnName, DeleteBehavior deleteBehavior)
    {
        var entity = _dbContext.Model.FindEntityType(typeof(TEntity));
        Assert.NotNull(entity);

        var matchedForeignKey = entity
            .GetForeignKeys()
            .SingleOrDefault(foreignKey =>
                foreignKey.DeleteBehavior == deleteBehavior
                && foreignKey.Properties.Select(property => property.GetColumnName()).SequenceEqual([columnName]));

        Assert.NotNull(matchedForeignKey);
    }
}
