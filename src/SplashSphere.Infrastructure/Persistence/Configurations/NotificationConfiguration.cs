using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(n => n.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.ReferenceId)
            .HasMaxLength(36);

        builder.Property(n => n.ReferenceType)
            .HasMaxLength(50);

        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(n => n.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(n => n.UpdatedAt)
            .IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(n => n.TenantId);

        // Fast unread count query
        builder.HasIndex(n => new { n.TenantId, n.IsRead });

        // Paginated list ordered by newest first
        builder.HasIndex(n => new { n.TenantId, n.CreatedAt })
            .IsDescending(false, true);
    }
}
