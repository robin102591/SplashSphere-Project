using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ServicePackageConfiguration : IEntityTypeConfiguration<ServicePackage>
{
    public void Configure(EntityTypeBuilder<ServicePackage> builder)
    {
        builder.ToTable("ServicePackages");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(sp => sp.Id);
        builder.Property(sp => sp.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(sp => sp.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sp => sp.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sp => sp.Description);

        builder.Property(sp => sp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(sp => sp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(sp => sp.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(sp => sp.Tenant)
            .WithMany()
            .HasForeignKey(sp => sp.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(sp => new { sp.Name, sp.TenantId }).IsUnique();
        builder.HasIndex(sp => sp.TenantId);
    }
}
