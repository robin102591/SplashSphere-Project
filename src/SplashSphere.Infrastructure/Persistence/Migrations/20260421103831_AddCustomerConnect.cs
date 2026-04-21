using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerConnect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Branches",
                type: "numeric(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Branches",
                type: "numeric(9,6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BranchId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    OpenTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CloseTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    SlotIntervalMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    MaxBookingsPerSlot = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    AdvanceBookingDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 7),
                    MinLeadTimeMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 120),
                    NoShowGraceMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    IsBookingEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ShowInPublicDirectory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingSettings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConnectUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AvatarUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlobalMakes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalMakes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReferrerCustomerId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    ReferredCustomerId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    ReferralCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReferrerPointsEarned = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ReferredPointsEarned = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referrals_Customers_ReferredCustomerId",
                        column: x => x.ReferredCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Referrals_Customers_ReferrerCustomerId",
                        column: x => x.ReferrerCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referrals_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConnectRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    ConnectUserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectRefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConnectRefreshTokens_ConnectUsers_ConnectUserId",
                        column: x => x.ConnectUserId,
                        principalTable: "ConnectUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConnectUserTenantLinks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    ConnectUserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LinkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectUserTenantLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConnectUserTenantLinks_ConnectUsers_ConnectUserId",
                        column: x => x.ConnectUserId,
                        principalTable: "ConnectUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConnectUserTenantLinks_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConnectUserTenantLinks_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GlobalModels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    GlobalMakeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalModels_GlobalMakes_GlobalMakeId",
                        column: x => x.GlobalMakeId,
                        principalTable: "GlobalMakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConnectVehicles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    ConnectUserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    MakeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    ModelId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    PlateNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConnectVehicles_ConnectUsers_ConnectUserId",
                        column: x => x.ConnectUserId,
                        principalTable: "ConnectUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConnectVehicles_GlobalMakes_MakeId",
                        column: x => x.MakeId,
                        principalTable: "GlobalMakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConnectVehicles_GlobalModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "GlobalModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BranchId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    ConnectUserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    ConnectVehicleId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CarId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    SlotStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SlotEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsVehicleClassified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EstimatedTotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    EstimatedTotalMin = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    EstimatedTotalMax = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    QueueEntryId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    TransactionId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Bookings_ConnectUsers_ConnectUserId",
                        column: x => x.ConnectUserId,
                        principalTable: "ConnectUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_ConnectVehicles_ConnectVehicleId",
                        column: x => x.ConnectVehicleId,
                        principalTable: "ConnectVehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_QueueEntries_QueueEntryId",
                        column: x => x.QueueEntryId,
                        principalTable: "QueueEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Bookings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BookingServices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BookingId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    ServiceId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    PriceMin = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    PriceMax = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingServices_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BranchId",
                table: "Bookings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CarId",
                table: "Bookings",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ConnectUserId",
                table: "Bookings",
                column: "ConnectUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ConnectVehicleId",
                table: "Bookings",
                column: "ConnectVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerId",
                table: "Bookings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_QueueEntryId",
                table: "Bookings",
                column: "QueueEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status_SlotStart",
                table: "Bookings",
                columns: new[] { "Status", "SlotStart" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TenantId_BranchId_SlotStart",
                table: "Bookings",
                columns: new[] { "TenantId", "BranchId", "SlotStart" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TransactionId",
                table: "Bookings",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingServices_BookingId",
                table: "BookingServices",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingServices_ServiceId",
                table: "BookingServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingSettings_BranchId",
                table: "BookingSettings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "UX_BookingSetting_Tenant_Branch",
                table: "BookingSettings",
                columns: new[] { "TenantId", "BranchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConnectRefreshTokens_ConnectUserId",
                table: "ConnectRefreshTokens",
                column: "ConnectUserId");

            migrationBuilder.CreateIndex(
                name: "UX_ConnectRefreshToken_TokenHash",
                table: "ConnectRefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConnectUsers_Email",
                table: "ConnectUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "UX_ConnectUser_Phone",
                table: "ConnectUsers",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConnectUserTenantLinks_CustomerId",
                table: "ConnectUserTenantLinks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectUserTenantLinks_TenantId",
                table: "ConnectUserTenantLinks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UX_ConnectUserTenantLink_User_Tenant",
                table: "ConnectUserTenantLinks",
                columns: new[] { "ConnectUserId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConnectVehicles_ConnectUserId",
                table: "ConnectVehicles",
                column: "ConnectUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectVehicles_MakeId",
                table: "ConnectVehicles",
                column: "MakeId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectVehicles_ModelId",
                table: "ConnectVehicles",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectVehicles_PlateNumber",
                table: "ConnectVehicles",
                column: "PlateNumber");

            migrationBuilder.CreateIndex(
                name: "UX_ConnectVehicle_User_Plate",
                table: "ConnectVehicles",
                columns: new[] { "ConnectUserId", "PlateNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_GlobalMake_Name",
                table: "GlobalMakes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_GlobalModel_Make_Name",
                table: "GlobalModels",
                columns: new[] { "GlobalMakeId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferredCustomerId",
                table: "Referrals",
                column: "ReferredCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferrerCustomerId",
                table: "Referrals",
                column: "ReferrerCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_Status_CreatedAt",
                table: "Referrals",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_TenantId_ReferrerCustomerId",
                table: "Referrals",
                columns: new[] { "TenantId", "ReferrerCustomerId" });

            migrationBuilder.CreateIndex(
                name: "UX_Referral_Tenant_Code",
                table: "Referrals",
                columns: new[] { "TenantId", "ReferralCode" },
                unique: true);

            // QueuePriority enum renumbered: Vip moved from 3 → 4 to make room
            // for the new Booked = 3 value (between Express=2 and Vip=4).
            // Promote any existing Vip=3 rows to the new value.
            migrationBuilder.Sql(@"UPDATE ""QueueEntries"" SET ""Priority"" = 4 WHERE ""Priority"" = 3;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingServices");

            migrationBuilder.DropTable(
                name: "BookingSettings");

            migrationBuilder.DropTable(
                name: "ConnectRefreshTokens");

            migrationBuilder.DropTable(
                name: "ConnectUserTenantLinks");

            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "ConnectVehicles");

            migrationBuilder.DropTable(
                name: "ConnectUsers");

            migrationBuilder.DropTable(
                name: "GlobalModels");

            migrationBuilder.DropTable(
                name: "GlobalMakes");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Branches");

            // Reverse QueuePriority renumber: Vip = 4 → 3.
            migrationBuilder.Sql(@"UPDATE ""QueueEntries"" SET ""Priority"" = 3 WHERE ""Priority"" = 4;");
        }
    }
}
