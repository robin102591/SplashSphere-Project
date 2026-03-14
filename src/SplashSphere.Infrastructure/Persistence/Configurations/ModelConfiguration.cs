using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ModelConfiguration : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        builder.ToTable("Models");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(m => m.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(m => m.MakeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(m => m.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Deleting a Make removes all its Models
        builder.HasOne(m => m.Make)
            .WithMany(mk => mk.Models)
            .HasForeignKey(m => m.MakeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Tenant)
            .WithMany()
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes ───────────────────────────────────────────────────────────
        // A make cannot have two models with the same name (e.g. two "Vios" under Toyota)
        builder.HasIndex(m => new { m.Name, m.MakeId }).IsUnique();
        builder.HasIndex(m => m.MakeId);
        builder.HasIndex(m => m.TenantId);
    }
}
