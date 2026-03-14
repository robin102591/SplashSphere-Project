using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(b => b.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.Address)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(b => b.ContactNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(b => b.Tenant)
            .WithMany(t => t.Branches)
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        // Branch codes are unique per tenant (e.g. two tenants can both have "MKT")
        builder.HasIndex(b => new { b.Code, b.TenantId }).IsUnique();
        builder.HasIndex(b => b.TenantId);
    }
}
