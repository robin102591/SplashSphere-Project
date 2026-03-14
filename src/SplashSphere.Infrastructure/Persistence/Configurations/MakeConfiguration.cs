using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class MakeConfiguration : IEntityTypeConfiguration<Make>
{
    public void Configure(EntityTypeBuilder<Make> builder)
    {
        builder.ToTable("Makes");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(m => m.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(m => m.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(m => m.Tenant)
            .WithMany(t => t.Makes)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        // A tenant cannot have two makes with the same name (e.g. two "Toyota" entries)
        builder.HasIndex(m => new { m.Name, m.TenantId }).IsUnique();
        builder.HasIndex(m => m.TenantId);
    }
}
