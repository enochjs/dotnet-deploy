using Domain.Entities.Pipelines.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Pipelines.Templates;

public sealed class PipelineTemplateConfiguration : IEntityTypeConfiguration<PipelineTemplate>
{
    public void Configure(EntityTypeBuilder<PipelineTemplate> builder)
    {
        builder.ToTable("pipeline_templates");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.TemplateKey).HasColumnName("template_key").HasMaxLength(128);
        builder.Property(entity => entity.Status).HasColumnName("status").HasDefaultValue(1);
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedByUserId).HasColumnName("updated_by_user_id").HasMaxLength(64);
        builder.Property(entity => entity.UpdatedByUserName).HasColumnName("updated_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => entity.Name)
            .HasDatabaseName("ix_pipeline_templates_name")
            .IsUnique();

        builder.HasIndex(entity => entity.TemplateKey)
            .HasDatabaseName("ix_pipeline_templates_template_key")
            .IsUnique();

        builder.HasMany(entity => entity.Stages)
            .WithOne(entity => entity.PipelineTemplate)
            .HasForeignKey(entity => entity.PipelineTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}