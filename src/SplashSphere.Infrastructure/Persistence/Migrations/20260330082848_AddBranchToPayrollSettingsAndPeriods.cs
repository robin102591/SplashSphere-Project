using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchToPayrollSettingsAndPeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayrollSettings_TenantId",
                table: "PayrollSettings");

            migrationBuilder.DropIndex(
                name: "IX_PayrollPeriods_TenantId_StartDate",
                table: "PayrollPeriods");

            migrationBuilder.AddColumn<string>(
                name: "BranchId",
                table: "PayrollSettings",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BranchId",
                table: "PayrollPeriods",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollSettings_BranchId",
                table: "PayrollSettings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollSettings_TenantId_BranchId",
                table: "PayrollSettings",
                columns: new[] { "TenantId", "BranchId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_BranchId",
                table: "PayrollPeriods",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_TenantId_BranchId_StartDate",
                table: "PayrollPeriods",
                columns: new[] { "TenantId", "BranchId", "StartDate" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollPeriods_Branches_BranchId",
                table: "PayrollPeriods",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollSettings_Branches_BranchId",
                table: "PayrollSettings",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayrollPeriods_Branches_BranchId",
                table: "PayrollPeriods");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollSettings_Branches_BranchId",
                table: "PayrollSettings");

            migrationBuilder.DropIndex(
                name: "IX_PayrollSettings_BranchId",
                table: "PayrollSettings");

            migrationBuilder.DropIndex(
                name: "IX_PayrollSettings_TenantId_BranchId",
                table: "PayrollSettings");

            migrationBuilder.DropIndex(
                name: "IX_PayrollPeriods_BranchId",
                table: "PayrollPeriods");

            migrationBuilder.DropIndex(
                name: "IX_PayrollPeriods_TenantId_BranchId_StartDate",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "PayrollSettings");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "PayrollPeriods");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollSettings_TenantId",
                table: "PayrollSettings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_TenantId_StartDate",
                table: "PayrollPeriods",
                columns: new[] { "TenantId", "StartDate" },
                unique: true);
        }
    }
}
