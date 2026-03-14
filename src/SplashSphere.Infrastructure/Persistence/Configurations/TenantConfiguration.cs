using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

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

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(t => t.UpdatedAt)
            .IsRequired();
        // UpdatedAt is managed by AuditableEntityInterceptor — no DB default.

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(t => t.Email).IsUnique();
    }
}
