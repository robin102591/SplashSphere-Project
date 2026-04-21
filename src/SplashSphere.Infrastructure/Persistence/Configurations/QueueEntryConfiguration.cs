using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class QueueEntryConfiguration : IEntityTypeConfiguration<QueueEntry>
{
    public void Configure(EntityTypeBuilder<QueueEntry> builder)
    {
        builder.ToTable("QueueEntries");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(qe => qe.Id);
        builder.Property(qe => qe.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(qe => qe.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(qe => qe.BranchId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(qe => qe.CustomerId)
            .HasMaxLength(36);

        builder.Property(qe => qe.CarId)
            .HasMaxLength(36);

        // FK for the optional one-to-one link to Transaction
        // Unique index enforces the one-to-one cardinality from the dependent side
        builder.Property(qe => qe.TransactionId)
            .HasMaxLength(26); // ULID

        // "Q-{DailySequence}" — e.g. "Q-042"
        builder.Property(qe => qe.QueueNumber)
            .IsRequired()
            .HasMaxLength(20);

        // Manila-local calendar date; determines the sequence reset boundary
        builder.Property(qe => qe.QueueDate)
            .IsRequired()
            .HasColumnType("date");

        // Always stored — copied from Car or entered manually for unregistered vehicles
        builder.Property(qe => qe.PlateNumber)
            .IsRequired()
            .HasMaxLength(20);

        // Stored as int — Waiting=1, Called=2, InService=3, Completed=4, Cancelled=5, NoShow=6
        builder.Property(qe => qe.Status)
            .IsRequired()
            .HasConversion<int>();

        // Stored as int — Regular=1, Express=2, Booked=3, Vip=4 (encodes sort order)
        builder.Property(qe => qe.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(qe => qe.EstimatedWaitMinutes);

        // JSON array of preferred service IDs — null means no preference
        builder.Property(qe => qe.PreferredServices);

        builder.Property(qe => qe.Notes);

        // ── Lifecycle timestamps (all UTC) ─────────────────────────────────────
        builder.Property(qe => qe.CalledAt);
        builder.Property(qe => qe.StartedAt);
        builder.Property(qe => qe.CompletedAt);
        builder.Property(qe => qe.CancelledAt);
        builder.Property(qe => qe.NoShowAt);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(qe => qe.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(qe => qe.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(qe => qe.Tenant)
            .WithMany(t => t.QueueEntries)
            .HasForeignKey(qe => qe.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cascade: queue entries are owned by the branch
        builder.HasOne(qe => qe.Branch)
            .WithMany(b => b.QueueEntries)
            .HasForeignKey(qe => qe.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // SetNull: customer can be deleted without losing queue history
        builder.HasOne(qe => qe.Customer)
            .WithMany(c => c.QueueEntries)
            .HasForeignKey(qe => qe.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: car record can be deleted; PlateNumber column preserves display value
        builder.HasOne(qe => qe.Car)
            .WithMany(c => c.QueueEntries)
            .HasForeignKey(qe => qe.CarId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // One-to-one (optional): QueueEntry is the dependent side; FK is TransactionId
        // SetNull: transaction deletion clears the link but keeps the queue record
        builder.HasOne(qe => qe.Transaction)
            .WithOne(t => t.QueueEntry)
            .HasForeignKey<QueueEntry>(qe => qe.TransactionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ── Unique constraints ────────────────────────────────────────────────
        // Queue number resets daily — unique per tenant, branch, and calendar day.
        // QueueDate (Manila local date) is the day-boundary column, so Q-001 on
        // 2026-03-20 never conflicts with Q-001 on 2026-03-21.
        builder.HasIndex(qe => new { qe.TenantId, qe.BranchId, qe.QueueDate, qe.QueueNumber })
            .IsUnique();

        // Enforces one-to-one cardinality: a transaction can only link to one queue entry
        builder.HasIndex(qe => qe.TransactionId)
            .IsUnique()
            .HasFilter("\"TransactionId\" IS NOT NULL"); // partial index: skip null FK rows

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(qe => qe.TenantId);
        builder.HasIndex(qe => qe.BranchId);

        // Queue board: current WAITING/CALLED/IN_SERVICE entries per branch, ordered by priority + createdAt
        builder.HasIndex(qe => new { qe.BranchId, qe.Status, qe.CreatedAt });

        // No-show Hangfire job: looks up entry by id after 5-min delay to check if still CALLED
        builder.HasIndex(qe => new { qe.TenantId, qe.Status });
    }
}
