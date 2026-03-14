using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.ToTable("ServiceCategories");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(sc => sc.Id);
        builder.Property(sc => sc.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(sc => sc.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sc => sc.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sc => sc.Description);
        // Description is unbounded text — no HasMaxLength

        builder.Property(sc => sc.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(sc => sc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(sc => sc.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(sc => sc.Tenant)
            .WithMany(t => t.ServiceCategories)
            .HasForeignKey(sc => sc.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(sc => new { sc.Name, sc.TenantId }).IsUnique();
        builder.HasIndex(sc => sc.TenantId);
    }
}
