using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId).IsRequired().HasMaxLength(128);
        builder.Property(p => p.UserId).IsRequired().HasMaxLength(128);
        builder.Property(p => p.NotificationType).IsRequired();

        // One preference per user per notification type
        builder.HasIndex(p => new { p.TenantId, p.UserId, p.NotificationType }).IsUnique();

        // For querying all preferences for a user
        builder.HasIndex(p => new { p.TenantId, p.UserId });
    }
}
