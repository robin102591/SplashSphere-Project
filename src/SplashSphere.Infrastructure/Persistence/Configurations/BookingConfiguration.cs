using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

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

        builder.Property(b => b.BranchId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(b => b.CustomerId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(b => b.ConnectUserId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(b => b.ConnectVehicleId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(b => b.CarId)
            .HasMaxLength(36);

        builder.Property(b => b.SlotStart)
            .IsRequired();

        builder.Property(b => b.SlotEnd)
            .IsRequired();

        // Stored as int — Confirmed=1, Arrived=2, InService=3, Completed=4, Cancelled=5, NoShow=6
        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(b => b.IsVehicleClassified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.EstimatedTotal)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.EstimatedTotalMin)
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.EstimatedTotalMax)
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.EstimatedDurationMinutes)
            .IsRequired();

        // FK to QueueEntry — set when the pre-queue job fires
        builder.Property(b => b.QueueEntryId)
            .HasMaxLength(36);

        // FK to Transaction (ULID) — set when service starts
        builder.Property(b => b.TransactionId)
            .HasMaxLength(26);

        builder.Property(b => b.CancellationReason)
            .HasMaxLength(500);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(b => b.Tenant)
            .WithMany()
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Branch)
            .WithMany()
            .HasForeignKey(b => b.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: deleting a Customer with bookings should be blocked
        builder.HasOne(b => b.Customer)
            .WithMany()
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.ConnectUser)
            .WithMany()
            .HasForeignKey(b => b.ConnectUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.ConnectVehicle)
            .WithMany()
            .HasForeignKey(b => b.ConnectVehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Car is optional — cashier-assigned at arrival for first-visit vehicles
        builder.HasOne(b => b.Car)
            .WithMany()
            .HasForeignKey(b => b.CarId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // QueueEntry + Transaction are optional forward links — FK managed from this side
        builder.HasOne(b => b.QueueEntry)
            .WithMany()
            .HasForeignKey(b => b.QueueEntryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(b => b.Transaction)
            .WithMany()
            .HasForeignKey(b => b.TransactionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(b => b.Services)
            .WithOne(s => s.Booking)
            .HasForeignKey(s => s.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(b => new { b.TenantId, b.BranchId, b.SlotStart });
        builder.HasIndex(b => b.ConnectUserId);
        // Used by CreateQueueFromBookings + MarkNoShows Hangfire jobs
        builder.HasIndex(b => new { b.Status, b.SlotStart });
    }
}
