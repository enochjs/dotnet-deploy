using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class DeployConfiguration : IEntityTypeConfiguration<Deploy>
{
    public void Configure(EntityTypeBuilder<Deploy> builder)
    {
        builder.ToTable("deploys");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedOnAdd();
        builder.Property(entity => entity.PipelineId).HasColumnName("pipeline_id").HasColumnType("uuid");
        builder.Property(entity => entity.AppKey).HasColumnName("app_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.IterationId).HasColumnName("iteration_id");
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.Env).HasColumnName("env").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Version).HasColumnName("version").HasMaxLength(256);
        builder.Property(entity => entity.UseVpn).HasColumnName("use_vpn");
        builder.Property(entity => entity.DeployType).HasColumnName("deploy_type");
        builder.Property(entity => entity.SwimLane).HasColumnName("swim_lane").HasMaxLength(128);
        builder.Property(entity => entity.IntegrationReleaseVersion).HasColumnName("integration_release_version").HasMaxLength(128);

        builder.HasIndex(entity => entity.AppKey)
            .HasDatabaseName("ix_deploys_app_key");

        builder.HasIndex(entity => entity.PipelineId)
            .HasDatabaseName("ix_deploys_pipeline_id");

        builder.HasOne(entity => entity.Pipeline)
            .WithMany()
            .HasForeignKey(entity => entity.PipelineId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}