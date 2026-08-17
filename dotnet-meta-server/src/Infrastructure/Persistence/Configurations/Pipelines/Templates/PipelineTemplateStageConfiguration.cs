using Domain.Entities.Pipelines.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Pipelines.Templates;

public sealed class PipelineTemplateStageConfiguration : IEntityTypeConfiguration<PipelineTemplateStage>
{
    public void Configure(EntityTypeBuilder<PipelineTemplateStage> builder)
    {
        builder.ToTable("pipeline_template_stages");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.PipelineTemplateId).HasColumnName("pipeline_template_id");
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Seq).HasColumnName("seq");

        builder.HasMany(entity => entity.Jobs)
            .WithOne(entity => entity.PipelineTemplateStage)
            .HasForeignKey(entity => entity.PipelineTemplateStageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}