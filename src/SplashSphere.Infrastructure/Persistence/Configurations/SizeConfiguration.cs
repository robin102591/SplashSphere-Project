using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class SizeConfiguration : IEntityTypeConfiguration<Size>
{
    public void Configure(EntityTypeBuilder<Size> builder)
    {
        builder.ToTable("Sizes");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(s => s.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(s => s.Tenant)
            .WithMany(t => t.Sizes)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        // A tenant cannot have two sizes with the same name
        builder.HasIndex(s => new { s.Name, s.TenantId }).IsUnique();
        builder.HasIndex(s => s.TenantId);
    }
}
