using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class AppMonitorConfiguration : IEntityTypeConfiguration<AppMonitor>
{
    public void Configure(EntityTypeBuilder<AppMonitor> builder)
    {
        builder.ToTable("app_monitors");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedOnAdd();
        builder.Property(entity => entity.AppKey).HasColumnName("app_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Env).HasColumnName("env").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Version).HasColumnName("version").HasMaxLength(256);
        builder.Property(entity => entity.SourceUuid).HasColumnName("source_uuid").HasMaxLength(64);
        builder.Property(entity => entity.TenantId).HasColumnName("tenant_id").HasMaxLength(64);
        builder.Property(entity => entity.TenantName).HasColumnName("tenant_name").HasMaxLength(128);
        builder.Property(entity => entity.UserId).HasColumnName("user_id").HasMaxLength(64);
        builder.Property(entity => entity.UserName).HasColumnName("user_name").HasMaxLength(64);
        builder.Property(entity => entity.Url).HasColumnName("url").HasMaxLength(2048);
        builder.Property(entity => entity.Browser).HasColumnName("browser").HasMaxLength(512);
        builder.Property(entity => entity.Message).HasColumnName("message").HasColumnType("text");
        builder.Property(entity => entity.Stack).HasColumnName("stack").HasColumnType("text");
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.Remark).HasColumnName("remark").HasMaxLength(2048);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.ResolvedByUserId).HasColumnName("resolved_by_user_id").HasMaxLength(64);
        builder.Property(entity => entity.ResolvedByUserName).HasColumnName("resolved_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.ResolvedAt).HasColumnName("resolved_at").HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => new
            {
                entity.AppKey,
                entity.Env,
            })
            .HasDatabaseName("ix_app_monitors_app_key_env");

        builder.HasIndex(entity => entity.Status)
            .HasDatabaseName("ix_app_monitors_status");
    }
}
