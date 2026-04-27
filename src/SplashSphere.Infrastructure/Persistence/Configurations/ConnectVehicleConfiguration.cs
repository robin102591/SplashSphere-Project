using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ConnectVehicleConfiguration : IEntityTypeConfiguration<ConnectVehicle>
{
    public void Configure(EntityTypeBuilder<ConnectVehicle> builder)
    {
        builder.ToTable("ConnectVehicles");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(v => v.ConnectUserId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(v => v.MakeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(v => v.ModelId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(v => v.PlateNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.Color)
            .HasMaxLength(50);

        builder.Property(v => v.Year);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(v => v.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(v => v.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(v => v.ConnectUser)
            .WithMany(u => u.Vehicles)
            .HasForeignKey(v => v.ConnectUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict delete on GlobalMake/GlobalModel — we never remove these,
        // and referential integrity protects customer data if someone tries.
        builder.HasOne(v => v.Make)
            .WithMany()
            .HasForeignKey(v => v.MakeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Model)
            .WithMany()
            .HasForeignKey(v => v.ModelId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(v => v.ConnectUserId);

        // One plate per user — you can't register the same car twice to yourself
        builder.HasIndex(v => new { v.ConnectUserId, v.PlateNumber })
            .IsUnique()
            .HasDatabaseName("UX_ConnectVehicle_User_Plate");

        // Lookup by plate supports tenant-side "first visit?" check
        builder.HasIndex(v => v.PlateNumber);
    }
}
