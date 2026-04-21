using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ConnectUserTenantLinkConfiguration : IEntityTypeConfiguration<ConnectUserTenantLink>
{
    public void Configure(EntityTypeBuilder<ConnectUserTenantLink> builder)
    {
        builder.ToTable("ConnectUserTenantLinks");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(l => l.ConnectUserId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(l => l.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.CustomerId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.LinkedAt)
            .IsRequired();

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(l => l.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(l => l.ConnectUser)
            .WithMany(u => u.TenantLinks)
            .HasForeignKey(l => l.ConnectUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Tenant)
            .WithMany()
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: deleting a tenant Customer row should require explicitly unlinking first
        builder.HasOne(l => l.Customer)
            .WithMany()
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes ───────────────────────────────────────────────────────────
        // One link per (user, tenant) — a customer cannot be linked twice to the same tenant
        builder.HasIndex(l => new { l.ConnectUserId, l.TenantId })
            .IsUnique()
            .HasDatabaseName("UX_ConnectUserTenantLink_User_Tenant");

        builder.HasIndex(l => l.TenantId);
        builder.HasIndex(l => l.CustomerId);
    }
}
