using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ConnectUserConfiguration : IEntityTypeConfiguration<ConnectUser>
{
    public void Configure(EntityTypeBuilder<ConnectUser> builder)
    {
        builder.ToTable("ConnectUsers");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        // E.164-normalized PH mobile (e.g. "+639171234567") — globally unique
        builder.Property(u => u.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.Email)
            .HasMaxLength(256);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(1024);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // Phone is the canonical global identifier — unique across all tenants
        builder.HasIndex(u => u.Phone)
            .IsUnique()
            .HasDatabaseName("UX_ConnectUser_Phone");

        // Email is optional; index supports lookup but is not unique
        builder.HasIndex(u => u.Email);
    }
}
