using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

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

        builder.Property(s => s.CategoryId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.Description);

        builder.Property(s => s.BasePrice)
            .IsRequired()
            .HasPrecision(10, 2);

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
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: a category with services cannot be deleted
        builder.HasOne(s => s.Category)
            .WithMany(c => c.Services)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(s => new { s.Name, s.TenantId }).IsUnique();
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.CategoryId);
    }
}
