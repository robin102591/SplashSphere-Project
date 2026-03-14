using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(u => u.ClerkUserId)
            .IsRequired()
            .HasMaxLength(256);

        // TenantId is nullable — null while user is pre-onboarding or invited
        builder.Property(u => u.TenantId)
            .HasMaxLength(256);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(u => u.Role)
            .HasMaxLength(100);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Computed / ignored ────────────────────────────────────────────────
        builder.Ignore(u => u.FullName);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // SetNull: deleting a tenant clears TenantId but keeps the user record
        builder.HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ── Indexes ───────────────────────────────────────────────────────────
        // ClerkUserId is a global unique identifier — not scoped to any tenant
        builder.HasIndex(u => u.ClerkUserId).IsUnique();
        builder.HasIndex(u => u.TenantId);
        builder.HasIndex(u => u.Email);
    }
}
