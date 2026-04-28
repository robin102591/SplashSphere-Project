using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        // ── Primary key ───────────────────────────────────────────────────────
        // ID is the Clerk Organization ID — always set by the application.
        // No gen_random_uuid() default; the DB must never generate this value.
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .IsRequired()
            .HasMaxLength(256)
            .ValueGeneratedNever();

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.ContactNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Address)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Branding ──────────────────────────────────────────────────────────
        builder.Property(t => t.Tagline)
            .HasMaxLength(200);

        builder.Property(t => t.Website)
            .HasMaxLength(256);

        // ── Structured address ────────────────────────────────────────────────
        builder.Property(t => t.StreetAddress).HasMaxLength(256);
        builder.Property(t => t.Barangay).HasMaxLength(128);
        builder.Property(t => t.City).HasMaxLength(128);
        builder.Property(t => t.Province).HasMaxLength(128);
        builder.Property(t => t.ZipCode).HasMaxLength(20);

        // ── Tax flag ──────────────────────────────────────────────────────────
        builder.Property(t => t.IsVatRegistered)
            .IsRequired()
            .HasDefaultValue(false);

        // ── Social & payment ──────────────────────────────────────────────────
        builder.Property(t => t.FacebookUrl).HasMaxLength(256);
        builder.Property(t => t.InstagramHandle).HasMaxLength(64);
        builder.Property(t => t.GCashNumber).HasMaxLength(50);

        // ── Logo URLs (Cloudflare R2) ────────────────────────────────────────
        builder.Property(t => t.LogoUrl).HasMaxLength(512);
        builder.Property(t => t.LogoThumbnailUrl).HasMaxLength(512);
        builder.Property(t => t.LogoIconUrl).HasMaxLength(512);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(t => t.UpdatedAt)
            .IsRequired();
        // UpdatedAt is managed by AuditableEntityInterceptor — no DB default.

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(t => t.Email).IsUnique();

        // ── Franchise properties ──────────────────────────────────────────────
        builder.Property(t => t.TenantType)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(TenantType.Independent);

        builder.Property(t => t.ParentTenantId)
            .HasMaxLength(256);

        builder.Property(t => t.FranchiseCode)
            .HasMaxLength(50);

        builder.Property(t => t.TaxId)
            .HasMaxLength(50);

        builder.Property(t => t.BusinessPermitNo)
            .HasMaxLength(100);

        // ── Franchise relationships ───────────────────────────────────────────
        builder.HasOne(t => t.ParentTenant)
            .WithMany(t => t.ChildTenants)
            .HasForeignKey(t => t.ParentTenantId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(t => t.ParentTenantId);
        builder.HasIndex(t => t.FranchiseCode)
            .IsUnique()
            .HasFilter("\"FranchiseCode\" IS NOT NULL");
    }
}
