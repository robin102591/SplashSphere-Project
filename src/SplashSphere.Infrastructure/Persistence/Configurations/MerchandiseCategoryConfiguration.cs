using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class MerchandiseCategoryConfiguration : IEntityTypeConfiguration<MerchandiseCategory>
{
    public void Configure(EntityTypeBuilder<MerchandiseCategory> builder)
    {
        builder.ToTable("MerchandiseCategories");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(mc => mc.Id);
        builder.Property(mc => mc.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(mc => mc.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(mc => mc.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(mc => mc.Description);
        // Description is unbounded text — no HasMaxLength

        builder.Property(mc => mc.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(mc => mc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(mc => mc.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(mc => mc.Tenant)
            .WithMany(t => t.MerchandiseCategories)
            .HasForeignKey(mc => mc.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(mc => new { mc.Name, mc.TenantId }).IsUnique();
        builder.HasIndex(mc => mc.TenantId);
    }
}
