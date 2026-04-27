using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class BookingServiceConfiguration : IEntityTypeConfiguration<BookingService>
{
    public void Configure(EntityTypeBuilder<BookingService> builder)
    {
        builder.ToTable("BookingServices");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(s => s.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.BookingId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(s => s.ServiceId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(s => s.Price)
            .HasColumnType("decimal(10,2)");

        builder.Property(s => s.PriceMin)
            .HasColumnType("decimal(10,2)");

        builder.Property(s => s.PriceMax)
            .HasColumnType("decimal(10,2)");

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Booking is configured from BookingConfiguration.HasMany(Services).WithOne(Booking)

        builder.HasOne(s => s.Service)
            .WithMany()
            .HasForeignKey(s => s.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(s => s.BookingId);
    }
}
