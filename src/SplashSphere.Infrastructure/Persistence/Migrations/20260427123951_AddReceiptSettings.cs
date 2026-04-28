using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReceiptSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BranchId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    ShowLogo = table.Column<bool>(type: "boolean", nullable: false),
                    LogoSize = table.Column<int>(type: "integer", nullable: false),
                    LogoPosition = table.Column<int>(type: "integer", nullable: false),
                    ShowBusinessName = table.Column<bool>(type: "boolean", nullable: false),
                    ShowTagline = table.Column<bool>(type: "boolean", nullable: false),
                    ShowBranchName = table.Column<bool>(type: "boolean", nullable: false),
                    ShowBranchAddress = table.Column<bool>(type: "boolean", nullable: false),
                    ShowBranchContact = table.Column<bool>(type: "boolean", nullable: false),
                    ShowTIN = table.Column<bool>(type: "boolean", nullable: false),
                    CustomHeaderText = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ShowServiceDuration = table.Column<bool>(type: "boolean", nullable: false),
                    ShowEmployeeNames = table.Column<bool>(type: "boolean", nullable: false),
                    ShowVehicleInfo = table.Column<bool>(type: "boolean", nullable: false),
                    ShowDiscountBreakdown = table.Column<bool>(type: "boolean", nullable: false),
                    ShowTaxLine = table.Column<bool>(type: "boolean", nullable: false),
                    ShowTransactionNumber = table.Column<bool>(type: "boolean", nullable: false),
                    ShowDateTime = table.Column<bool>(type: "boolean", nullable: false),
                    ShowCashierName = table.Column<bool>(type: "boolean", nullable: false),
                    ShowCustomerName = table.Column<bool>(type: "boolean", nullable: false),
                    ShowCustomerPhone = table.Column<bool>(type: "boolean", nullable: false),
                    ShowLoyaltyPointsEarned = table.Column<bool>(type: "boolean", nullable: false),
                    ShowLoyaltyBalance = table.Column<bool>(type: "boolean", nullable: false),
                    ShowLoyaltyTier = table.Column<bool>(type: "boolean", nullable: false),
                    ThankYouMessage = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValue: "Thank you for your patronage!"),
                    PromoText = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ShowSocialMedia = table.Column<bool>(type: "boolean", nullable: false),
                    ShowGCashQr = table.Column<bool>(type: "boolean", nullable: false),
                    ShowGCashNumber = table.Column<bool>(type: "boolean", nullable: false),
                    CustomFooterText = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ReceiptWidth = table.Column<int>(type: "integer", nullable: false),
                    FontSize = table.Column<int>(type: "integer", nullable: false),
                    AutoCutPaper = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptSettings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceiptSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptSettings_BranchId",
                table: "ReceiptSettings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptSettings_TenantId",
                table: "ReceiptSettings",
                column: "TenantId",
                unique: true,
                filter: "\"BranchId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptSettings_TenantId_BranchId",
                table: "ReceiptSettings",
                columns: new[] { "TenantId", "BranchId" },
                unique: true,
                filter: "\"BranchId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiptSettings");
        }
    }
}
