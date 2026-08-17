using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class SubApplicationConfiguration : IEntityTypeConfiguration<SubApplication>
{
    public void Configure(EntityTypeBuilder<SubApplication> builder)
    {
        builder.ToTable("sub_applications");
        // 指定主键
        builder.HasKey(e => e.Id);

        // 指定字段
        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.ParentApplicationId).HasColumnName("parent_application_id").IsRequired();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.AppKey).HasColumnName("app_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Platform).HasColumnName("platform").HasMaxLength(32);
        builder.Property(entity => entity.DeployKey).HasColumnName("deploy_key").HasMaxLength(128);
        builder.Property(entity => entity.GitId).HasColumnName("git_id");
        builder.Property(entity => entity.RegistryKey).HasColumnName("registry_key").HasMaxLength(64).HasDefaultValue("fe").IsRequired();
        builder.Property(entity => entity.GitName).HasColumnName("git_name").HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.GitRepo).HasColumnName("git_repo").HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.MainBranch).HasColumnName("main_branch").HasMaxLength(128).HasDefaultValue("main").IsRequired();
        builder.Property(entity => entity.PreBranch).HasColumnName("pre_branch").HasMaxLength(128).HasDefaultValue("pre").IsRequired();
        builder.Property(entity => entity.StageBranch).HasColumnName("stage_branch").HasMaxLength(128).HasDefaultValue("stage").IsRequired();
        builder.Property(entity => entity.DevBranch).HasColumnName("dev_branch").HasMaxLength(128).HasDefaultValue("dev").IsRequired();
        builder.Property(entity => entity.ProdSiteAddress).HasColumnName("prod_site_address").HasMaxLength(256);
        builder.Property(entity => entity.PreSiteAddress).HasColumnName("pre_site_address").HasMaxLength(256);
        builder.Property(entity => entity.StageSiteAddress).HasColumnName("stage_site_address").HasMaxLength(256);
        builder.Property(entity => entity.DevSiteAddress).HasColumnName("dev_site_address").HasMaxLength(256);
        builder.Property(entity => entity.GitNamespaceId).HasColumnName("git_namespace_id");
        builder.Property(entity => entity.TriggerToken).HasColumnName("trigger_token").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Remark).HasColumnName("remark").HasMaxLength(1024);
        builder.Property(entity => entity.PublicPath).HasColumnName("public_path").HasMaxLength(256);
        builder.Property(entity => entity.UploadToOss).HasColumnName("upload_to_oss").HasDefaultValue(false).IsRequired();
        builder.Property(entity => entity.AppType).HasColumnName("app_type").HasMaxLength(32).HasDefaultValue("saas").IsRequired();
        builder.Property(entity => entity.Variables).HasColumnName("variables").HasColumnType("jsonb");
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.UpdatedByUserId).HasColumnName("updated_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.UpdatedByUserName).HasColumnName("updated_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").HasDefaultValue(DateTimeOffset.UtcNow).IsRequired();
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone").HasDefaultValue(DateTimeOffset.UtcNow).IsRequired();

        // 索引
        builder.HasIndex(entity => entity.AppKey)
            .HasDatabaseName("ix_sub_applications_app_key")
            .IsUnique();

        // 关联父应用
        builder.HasOne(subApplication => subApplication.ParentApplication)
          .WithMany(application => application.SubApplications)
          .HasForeignKey(subApplication => subApplication.ParentApplicationId)
          .OnDelete(DeleteBehavior.Restrict);

    }
}