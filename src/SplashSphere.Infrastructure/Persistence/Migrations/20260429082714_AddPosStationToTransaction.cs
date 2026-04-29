using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPosStationToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PosStationId",
                table: "Transactions",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PosStationId_Status",
                table: "Transactions",
                columns: new[] { "PosStationId", "Status" },
                filter: "\"PosStationId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_PosStations_PosStationId",
                table: "Transactions",
                column: "PosStationId",
                principalTable: "PosStations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_PosStations_PosStationId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_PosStationId_Status",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PosStationId",
                table: "Transactions");
        }
    }
}
