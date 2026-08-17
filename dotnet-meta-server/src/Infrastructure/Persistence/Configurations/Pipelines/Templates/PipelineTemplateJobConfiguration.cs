using Domain.Entities.Pipelines.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Pipelines.Templates;

public sealed class PipelineTemplateJobConfiguration : IEntityTypeConfiguration<PipelineTemplateJob>
{
    public void Configure(EntityTypeBuilder<PipelineTemplateJob> builder)
    {
        builder.ToTable("pipeline_template_jobs");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.PipelineTemplateStageId).HasColumnName("pipeline_template_stage_id");
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.JobKey).HasColumnName("job_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.StageSeq).HasColumnName("stage_seq");
        builder.Property(entity => entity.Extra).HasColumnName("extra").HasColumnType("jsonb");
    }
}