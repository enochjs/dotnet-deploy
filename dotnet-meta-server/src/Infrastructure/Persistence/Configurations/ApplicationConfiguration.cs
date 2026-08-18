using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("applications");
        // 指定主键
        builder.HasKey(e => e.Id);
        
        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.AppKey).HasColumnName("app_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ProjectType).HasColumnName("project_type").HasMaxLength(32).HasDefaultValue("fe").IsRequired();
        builder.Property(entity => entity.DeployKey).HasColumnName("deploy_key").HasMaxLength(128);
        builder.Property(entity => entity.GitId).HasColumnName("git_id");
        builder.Property(entity => entity.RegistryKey).HasColumnName("registry_key").HasMaxLength(64).HasDefaultValue("fe").IsRequired();
        builder.Property(entity => entity.GitName).HasColumnName("git_name").HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.GitRepo).HasColumnName("git_repo").HasMaxLength(512).IsRequired();

        builder.Property(entity => entity.GitNamespaceId).HasColumnName("git_namespace_id");
        builder.Property(entity => entity.TriggerToken).HasColumnName("trigger_token").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.OwnerName).HasColumnName("owner_name").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.Remark).HasColumnName("remark").HasMaxLength(1024);
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedByUserId).HasColumnName("updated_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.UpdatedByUserName).HasColumnName("updated_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        // 索引
        builder.HasIndex(entity => entity.AppKey)
          .HasDatabaseName("ix_applications_app_key")
          .IsUnique();
    }
}