using Domain.Entities.Pipelines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Pipelines;

public sealed class PipelineJobConfiguration : IEntityTypeConfiguration<PipelineJob>
{
    public void Configure(EntityTypeBuilder<PipelineJob> builder)
    {
        builder.ToTable("pipeline_jobs");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedOnAdd();
        builder.Property(entity => entity.PipelineId).HasColumnName("pipeline_id").HasColumnType("uuid");
        builder.Property(entity => entity.StageSeq).HasColumnName("stage_seq");
        builder.Property(entity => entity.JobKey).HasColumnName("job_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UnitKey).HasColumnName("unit_key").HasMaxLength(256);
        builder.Property(entity => entity.Extra).HasColumnName("extra").HasColumnType("jsonb");

        builder.HasIndex(entity => new
            {
                entity.PipelineId,
                entity.StageSeq,
                entity.JobKey,
            })
            .HasDatabaseName("ix_pipeline_jobs_pipeline_stage_job")
            .IsUnique();
    }
}