using Domain.Entities.Pipelines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Pipelines;

public sealed class PipelineConfiguration : IEntityTypeConfiguration<Pipeline>
{
    public void Configure(EntityTypeBuilder<Pipeline> builder)
    {
        builder.ToTable("pipelines");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedOnAdd();
        builder.Property(entity => entity.AppKey).HasColumnName("app_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.IterationId).HasColumnName("iteration_id");
        builder.Property(entity => entity.RepoId).HasColumnName("repo_id");
        builder.Property(entity => entity.RegistryKey).HasColumnName("registry_key").HasMaxLength(64).HasDefaultValue("fe").IsRequired();
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.StageSeq).HasColumnName("stage_seq").HasDefaultValue(-1);
        builder.Property(entity => entity.PipelineTemplateId).HasColumnName("pipeline_template_id");
        builder.Property(entity => entity.Branch).HasColumnName("branch").HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Content).HasColumnName("content").HasMaxLength(2048);
        builder.Property(entity => entity.SwimLane).HasColumnName("swim_lane").HasMaxLength(128);
        builder.Property(entity => entity.ForceUpdate).HasColumnName("force_update");
        builder.Property(entity => entity.Extra).HasColumnName("extra").HasColumnType("jsonb");

        builder.HasIndex(entity => entity.IterationId)
            .HasDatabaseName("ix_pipelines_iteration_id");

        builder.HasIndex(entity => entity.AppKey)
            .HasDatabaseName("ix_pipelines_app_key");

        builder.HasOne(entity => entity.PipelineTemplate)
            .WithMany()
            .HasForeignKey(entity => entity.PipelineTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Jobs)
            .WithOne(entity => entity.Pipeline)
            .HasForeignKey(entity => entity.PipelineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}