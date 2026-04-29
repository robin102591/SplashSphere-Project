using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class DisplaySettingConfiguration : IEntityTypeConfiguration<DisplaySetting>
{
    public void Configure(EntityTypeBuilder<DisplaySetting> builder)
    {
        builder.ToTable("DisplaySettings");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(d => d.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(d => d.BranchId)
            .HasMaxLength(36);

        builder.HasOne(d => d.Tenant)
            .WithMany()
            .HasForeignKey(d => d.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Branch)
            .WithMany()
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // ── Promo messages — stored as JSONB so the resolver can hydrate the
        // ── list without a join. Few entries per row (typically 1-5). The
        // ── ValueComparer makes EF Core change-tracking notice list mutations
        // ── (otherwise reference equality would miss items added in-place).
        var stringListConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        var stringListComparer = new ValueComparer<List<string>>(
            (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
            v => v.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.GetHashCode())),
            v => v.ToList());

        builder.Property(d => d.PromoMessages)
            .HasColumnType("jsonb")
            .HasConversion(stringListConverter, stringListComparer)
            .IsRequired()
            .HasDefaultValueSql("'[]'::jsonb");

        // ── Enums (stored as int) ─────────────────────────────────────────────
        builder.Property(d => d.Theme).HasConversion<int>().IsRequired();
        builder.Property(d => d.FontSize).HasConversion<int>().IsRequired();
        builder.Property(d => d.Orientation).HasConversion<int>().IsRequired();

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(d => d.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // One row per (tenant, branch). Same partial-filtered-index pattern as
        // ReceiptSettings — Postgres treats NULL != NULL by default.
        builder.HasIndex(d => new { d.TenantId, d.BranchId })
            .IsUnique()
            .HasFilter("\"BranchId\" IS NOT NULL");

        builder.HasIndex(d => d.TenantId)
            .IsUnique()
            .HasFilter("\"BranchId\" IS NULL");
    }
}
