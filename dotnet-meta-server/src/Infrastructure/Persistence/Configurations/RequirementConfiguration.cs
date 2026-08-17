using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RequirementConfiguration : IEntityTypeConfiguration<Requirement>
{
    public void Configure(EntityTypeBuilder<Requirement> builder)
    {
        builder.ToTable("requirements");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.DocumentUrl).HasColumnName("document_url").HasMaxLength(1024).IsRequired();
        builder.Property(entity => entity.Priority).HasColumnName("priority");
        builder.Property(entity => entity.Remark).HasColumnName("remark").HasMaxLength(2048);
        builder.Property(entity => entity.OnlineAt).HasColumnName("online_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.SubmittedTestAt).HasColumnName("submitted_test_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedByUserId).HasColumnName("updated_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.UpdatedByUserName).HasColumnName("updated_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasMany(entity => entity.Developers)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "requirement_developers",
                right => right
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("user_id")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Requirement>()
                    .WithMany()
                    .HasForeignKey("requirement_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("requirement_developers");
                    join.HasKey("requirement_id", "user_id");
                    join.IndexerProperty<int>("requirement_id").HasColumnName("requirement_id");
                    join.IndexerProperty<int>("user_id").HasColumnName("user_id");
                });

        builder.HasMany(entity => entity.Followers)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "requirement_followers",
                right => right
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("user_id")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Requirement>()
                    .WithMany()
                    .HasForeignKey("requirement_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("requirement_followers");
                    join.HasKey("requirement_id", "user_id");
                    join.IndexerProperty<int>("requirement_id").HasColumnName("requirement_id");
                    join.IndexerProperty<int>("user_id").HasColumnName("user_id");
                });
    }
}