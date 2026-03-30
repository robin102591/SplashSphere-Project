using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(a => a.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.UserId)
            .HasMaxLength(256);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(a => a.Changes)
            .HasColumnType("jsonb");

        builder.Property(a => a.Timestamp)
            .IsRequired();

        // Paginated list: newest first
        builder.HasIndex(a => new { a.TenantId, a.Timestamp })
            .IsDescending(false, true);

        // Entity-specific lookups
        builder.HasIndex(a => new { a.TenantId, a.EntityType, a.EntityId });
    }
}
