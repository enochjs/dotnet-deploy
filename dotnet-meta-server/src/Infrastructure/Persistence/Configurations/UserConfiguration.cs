using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.UserId).HasColumnName("user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.DingTalkUserId).HasColumnName("ding_talk_user_id").HasMaxLength(64);
        builder.Property(entity => entity.ManagerUserId).HasColumnName("manager_user_id").HasMaxLength(64);
        builder.Property(entity => entity.ManagerDingTalkUserId).HasColumnName("manager_ding_talk_user_id").HasMaxLength(64);
        builder.Property(entity => entity.Email).HasColumnName("email").HasMaxLength(128);
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.PasswordHash).HasColumnName(("password_hash")).HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.RealName).HasColumnName("real_name").HasMaxLength(64);
        builder.Property(entity => entity.Mobile).HasColumnName("mobile").HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Role).HasColumnName("role");
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => entity.Mobile)
            .HasDatabaseName("ix_users_mobile")
            .IsUnique();
        
        builder.HasIndex(entity => entity.UserId)
            .HasDatabaseName("ix_users_user_id")
            .IsUnique();
    }
}