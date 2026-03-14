using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(c => c.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(c => c.Email)
            .HasMaxLength(256);

        builder.Property(c => c.ContactNumber)
            .HasMaxLength(50);

        builder.Property(c => c.Notes);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Computed / ignored ────────────────────────────────────────────────
        builder.Ignore(c => c.FullName);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(c => c.TenantId);

        // Email is not globally unique — two different tenants may register the
        // same customer. Index supports search-by-email within a tenant.
        builder.HasIndex(c => new { c.Email, c.TenantId });

        // Full-name lookups: individual column indexes cover both directions
        builder.HasIndex(c => new { c.LastName, c.TenantId });
    }
}
