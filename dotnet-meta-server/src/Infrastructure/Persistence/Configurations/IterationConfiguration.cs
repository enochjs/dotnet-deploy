using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class IterationConfiguration : IEntityTypeConfiguration<Iteration>
{
    public void Configure(EntityTypeBuilder<Iteration> builder)
    {
        builder.ToTable("iterations");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.ApplicationId).HasColumnName("application_id");
        builder.Property(entity => entity.SubApplicationId).HasColumnName("sub_application_id");
        builder.Property(entity => entity.IntegrationReleaseId).HasColumnName("integration_release_id");
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.ApplicationName).HasColumnName("application_name").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.SubApplicationName).HasColumnName("sub_application_name").HasMaxLength(128);
        builder.Property(entity => entity.Branch).HasColumnName("branch").HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.OriginalCommit).HasColumnName("original_commit").HasMaxLength(64);
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.Remark).HasColumnName("remark").HasMaxLength(2048);
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedByUserId).HasColumnName("updated_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.UpdatedByUserName).HasColumnName("updated_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasOne(entity => entity.Application)
            .WithMany()
            .HasForeignKey(entity => entity.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.SubApplication)
            .WithMany()
            .HasForeignKey(entity => entity.SubApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.IntegrationRelease)
            .WithMany()
            .HasForeignKey(entity => entity.IntegrationReleaseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(entity => entity.Requirements)
            .WithMany(entity => entity.Iterations)
            .UsingEntity<Dictionary<string, object>>(
                "iteration_requirements",
                right => right
                    .HasOne<Requirement>()
                    .WithMany()
                    .HasForeignKey("requirement_id")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Iteration>()
                    .WithMany()
                    .HasForeignKey("iteration_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("iteration_requirements");
                    join.HasKey("iteration_id", "requirement_id");
                    join.IndexerProperty<int>("iteration_id").HasColumnName("iteration_id");
                    join.IndexerProperty<int>("requirement_id").HasColumnName("requirement_id");
                });
    }
}