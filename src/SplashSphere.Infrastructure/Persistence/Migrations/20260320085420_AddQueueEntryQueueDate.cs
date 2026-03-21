using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQueueEntryQueueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QueueEntries_BranchId_QueueNumber_TenantId",
                table: "QueueEntries");

            migrationBuilder.AddColumn<DateOnly>(
                name: "QueueDate",
                table: "QueueEntries",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_TenantId_BranchId_QueueDate_QueueNumber",
                table: "QueueEntries",
                columns: new[] { "TenantId", "BranchId", "QueueDate", "QueueNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QueueEntries_TenantId_BranchId_QueueDate_QueueNumber",
                table: "QueueEntries");

            migrationBuilder.DropColumn(
                name: "QueueDate",
                table: "QueueEntries");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_BranchId_QueueNumber_TenantId",
                table: "QueueEntries",
                columns: new[] { "BranchId", "QueueNumber", "TenantId" },
                unique: true);
        }
    }
}
