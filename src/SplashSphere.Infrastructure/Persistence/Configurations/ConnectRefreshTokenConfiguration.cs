using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ConnectRefreshTokenConfiguration : IEntityTypeConfiguration<ConnectRefreshToken>
{
    public void Configure(EntityTypeBuilder<ConnectRefreshToken> builder)
    {
        builder.ToTable("ConnectRefreshTokens");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(t => t.ConnectUserId)
            .IsRequired()
            .HasMaxLength(36);

        // SHA-256 hex digest = 64 chars
        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(t => t.ExpiresAt)
            .IsRequired();

        builder.Property(t => t.RevokedAt);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(t => t.ConnectUser)
            .WithMany()
            .HasForeignKey(t => t.ConnectUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        // Lookup-by-hash for refresh validation
        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("UX_ConnectRefreshToken_TokenHash");

        builder.HasIndex(t => t.ConnectUserId);
    }
}
