using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PackageServiceConfiguration : IEntityTypeConfiguration<PackageService>
{
    public void Configure(EntityTypeBuilder<PackageService> builder)
    {
        builder.ToTable("PackageServices");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(ps => ps.Id);
        builder.Property(ps => ps.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(ps => ps.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ps => ps.PackageId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(ps => ps.ServiceId)
            .IsRequired()
            .HasMaxLength(36);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(ps => ps.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(ps => ps.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: link rows are owned by the package
        builder.HasOne(ps => ps.Package)
            .WithMany(p => p.PackageServices)
            .HasForeignKey(ps => ps.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: a service cannot be deleted while it belongs to a package
        builder.HasOne(ps => ps.Service)
            .WithMany(s => s.PackageServices)
            .HasForeignKey(ps => ps.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Unique constraint: a service can only appear once in a package ────
        builder.HasIndex(ps => new { ps.PackageId, ps.ServiceId })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(ps => ps.TenantId);
        builder.HasIndex(ps => ps.ServiceId);
    }
}
