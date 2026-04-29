using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplaySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DisplaySettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BranchId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    ShowLogo = table.Column<bool>(type: "boolean", nullable: false),
                    ShowBusinessName = table.Column<bool>(type: "boolean", nullable: false),
                    ShowTagline = table.Column<bool>(type: "boolean", nullable: false),
                    ShowDateTime = table.Column<bool>(type: "boolean", nullable: false),
                    ShowGCashQr = table.Column<bool>(type: "boolean", nullable: false),
                    ShowSocialMedia = table.Column<bool>(type: "boolean", nullable: false),
                    PromoMessages = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    PromoRotationSeconds = table.Column<int>(type: "integer", nullable: false),
                    ShowVehicleInfo = table.Column<bool>(type: "boolean", nullable: false),
                    ShowCustomerName = table.Column<bool>(type: "boolean", nullable: false),
                    ShowLoyaltyTier = table.Column<bool>(type: "boolean", nullable: false),
                    ShowDiscountBreakdown = table.Column<bool>(type: "boolean", nullable: false),
                    ShowTaxLine = table.Column<bool>(type: "boolean", nullable: false),
                    ShowPaymentMethod = table.Column<bool>(type: "boolean", nullable: false),
                    ShowChangeAmount = table.Column<bool>(type: "boolean", nullable: false),
                    ShowPointsEarned = table.Column<bool>(type: "boolean", nullable: false),
                    ShowPointsBalance = table.Column<bool>(type: "boolean", nullable: false),
                    ShowThankYouMessage = table.Column<bool>(type: "boolean", nullable: false),
                    ShowPromoText = table.Column<bool>(type: "boolean", nullable: false),
                    CompletionHoldSeconds = table.Column<int>(type: "integer", nullable: false),
                    Theme = table.Column<int>(type: "integer", nullable: false),
                    FontSize = table.Column<int>(type: "integer", nullable: false),
                    Orientation = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplaySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisplaySettings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DisplaySettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisplaySettings_BranchId",
                table: "DisplaySettings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplaySettings_TenantId",
                table: "DisplaySettings",
                column: "TenantId",
                unique: true,
                filter: "\"BranchId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DisplaySettings_TenantId_BranchId",
                table: "DisplaySettings",
                columns: new[] { "TenantId", "BranchId" },
                unique: true,
                filter: "\"BranchId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisplaySettings");
        }
    }
}
