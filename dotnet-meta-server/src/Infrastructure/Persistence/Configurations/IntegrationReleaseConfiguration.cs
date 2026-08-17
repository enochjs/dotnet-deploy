using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class IntegrationReleaseConfiguration : IEntityTypeConfiguration<IntegrationRelease>
{
    public void Configure(EntityTypeBuilder<IntegrationRelease> builder)
    {
        builder.ToTable("integration_releases");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.Version).HasColumnName("version").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(512);
        builder.Property(entity => entity.Branch).HasColumnName("branch").HasMaxLength(256);
        builder.Property(entity => entity.Remark).HasColumnName("remark").HasMaxLength(2048);
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => entity.Version)
            .HasDatabaseName("ix_integration_releases_version")
            .IsUnique();

        builder.HasMany(entity => entity.ReleaseApps)
            .WithOne(entity => entity.IntegrationRelease)
            .HasForeignKey(entity => entity.IntegrationReleaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}