using Domain.Entities;
using Domain.Entities.Pipelines;
using Domain.Entities.Pipelines.Templates;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class MetaServerDbContext(DbContextOptions<MetaServerDbContext> options) : DbContext(options)
{
  public DbSet<Application> Applications => Set<Application>();
  public DbSet<SubApplication> SubApplications => Set<SubApplication>();
  public DbSet<User> Users => Set<User>();
  public DbSet<Requirement> Requirements => Set<Requirement>();
  public DbSet<Iteration> Iterations => Set<Iteration>();
  public DbSet<IntegrationRelease> IntegrationReleases => Set<IntegrationRelease>();
  public DbSet<IntegrationReleaseApp> IntegrationReleaseApps => Set<IntegrationReleaseApp>();
  public DbSet<PipelineTemplate> PipelineTemplates => Set<PipelineTemplate>();
  public DbSet<PipelineTemplateStage> PipelineTemplateStages => Set<PipelineTemplateStage>();
  public DbSet<PipelineTemplateJob> PipelineTemplateJobs => Set<PipelineTemplateJob>();
  public DbSet<Pipeline> Pipelines => Set<Pipeline>();
  public DbSet<PipelineJob> PipelineJobs => Set<PipelineJob>();
  public DbSet<Deploy> Deploys => Set<Deploy>();
  public DbSet<Monitor> Monitors => Set<Monitor>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(MetaServerDbContext).Assembly);
  }
}