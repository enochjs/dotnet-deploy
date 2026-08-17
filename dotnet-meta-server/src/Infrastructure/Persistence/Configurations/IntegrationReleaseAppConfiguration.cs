using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class IntegrationReleaseAppConfiguration : IEntityTypeConfiguration<IntegrationReleaseApp>
{
    public void Configure(EntityTypeBuilder<IntegrationReleaseApp> builder)
    {
        builder.ToTable("integration_release_apps");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.IntegrationReleaseId).HasColumnName("integration_release_id");
        builder.Property(entity => entity.ApplicationId).HasColumnName("application_id");
        builder.Property(entity => entity.SubApplicationId).HasColumnName("sub_application_id");
        builder.Property(entity => entity.AppKey).HasColumnName("app_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ApplicationName).HasColumnName("application_name").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.SubApplicationName).HasColumnName("sub_application_name").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.IterationId).HasColumnName("iteration_id");

        builder.HasOne(entity => entity.Application)
            .WithMany()
            .HasForeignKey(entity => entity.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.SubApplication)
            .WithMany()
            .HasForeignKey(entity => entity.SubApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Iteration)
            .WithMany()
            .HasForeignKey(entity => entity.IterationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}