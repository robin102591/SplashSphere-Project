using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralRewardSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReferredRewardPoints",
                table: "LoyaltyProgramSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferrerRewardPoints",
                table: "LoyaltyProgramSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentAt",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferredRewardPoints",
                table: "LoyaltyProgramSettings");

            migrationBuilder.DropColumn(
                name: "ReferrerRewardPoints",
                table: "LoyaltyProgramSettings");

            migrationBuilder.DropColumn(
                name: "ReminderSentAt",
                table: "Bookings");
        }
    }
}
