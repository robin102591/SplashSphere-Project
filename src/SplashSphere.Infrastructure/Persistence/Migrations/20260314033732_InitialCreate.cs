using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ContactNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ContactNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Makes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Makes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Makes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MerchandiseCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchandiseCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchandiseCategories_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPeriods",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    CutOffWeek = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPeriods_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceCategories_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServicePackages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePackages_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sizes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sizes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    ClerkUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VehicleTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleTypes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BranchId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ContactNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EmployeeType = table.Column<int>(type: "integer", nullable: false),
                    DailyRate = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    HiredDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PricingModifiers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BranchId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ActiveDayOfWeek = table.Column<int>(type: "integer", nullable: true),
                    HolidayDate = table.Column<DateOnly>(type: "date", nullable: true),
                    HolidayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingModifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PricingModifiers_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PricingModifiers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MakeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Models_Makes_MakeId",
                        column: x => x.MakeId,
                        principalTable: "Makes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Models_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Merchandise",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CategoryId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchandise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Merchandise_MerchandiseCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "MerchandiseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Merchandise_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CategoryId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    BasePrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_ServiceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Services_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageCommissions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PackageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    VehicleTypeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    SizeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    PercentageRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageCommissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageCommissions_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageCommissions_Sizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "Sizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageCommissions_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackagePricing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PackageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    VehicleTypeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    SizeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagePricing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackagePricing_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackagePricing_Sizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "Sizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePricing_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EmployeeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TimeIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PayrollPeriodId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    EmployeeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    EmployeeTypeSnapshot = table.Column<int>(type: "integer", nullable: false),
                    DaysWorked = table.Column<int>(type: "integer", nullable: false),
                    DailyRateSnapshot = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    BaseSalary = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalCommissions = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Bonuses = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    Deductions = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollEntries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollEntries_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollEntries_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    VehicleTypeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    SizeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    MakeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    ModelId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    PlateNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cars_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Cars_Makes_MakeId",
                        column: x => x.MakeId,
                        principalTable: "Makes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Cars_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Cars_Sizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "Sizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cars_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cars_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageServices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PackageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    ServiceId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageServices_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCommissions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ServiceId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    VehicleTypeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    SizeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    FixedAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    PercentageRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCommissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceCommissions_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceCommissions_Sizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "Sizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceCommissions_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServicePricing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ServiceId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    VehicleTypeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    SizeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePricing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePricing_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicePricing_Sizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "Sizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServicePricing_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BranchId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CashierId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CarId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    TransactionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FinalAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Transactions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Users_CashierId",
                        column: x => x.CashierId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueueEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BranchId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    CarId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    TransactionId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: true),
                    QueueNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PlateNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    EstimatedWaitMinutes = table.Column<int>(type: "integer", nullable: true),
                    PreferredServices = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NoShowAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueueEntries_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueueEntries_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QueueEntries_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QueueEntries_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueueEntries_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TransactionEmployees",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    EmployeeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    TotalCommission = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionEmployees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionEmployees_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionEmployees_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionMerchandise",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    MerchandiseId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionMerchandise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionMerchandise_Merchandise_MerchandiseId",
                        column: x => x.MerchandiseId,
                        principalTable: "Merchandise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionMerchandise_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionPackages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    PackageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    VehicleTypeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    SizeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalCommission = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionPackages_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionPackages_Sizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "Sizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionPackages_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionPackages_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionServices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    ServiceId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    VehicleTypeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    SizeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalCommission = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionServices_Sizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "Sizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionServices_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionServices_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageEmployeeAssignments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TransactionPackageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    EmployeeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEmployeeAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEmployeeAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageEmployeeAssignments_TransactionPackages_TransactionP~",
                        column: x => x.TransactionPackageId,
                        principalTable: "TransactionPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceEmployeeAssignments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TransactionServiceId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    EmployeeId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceEmployeeAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceEmployeeAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceEmployeeAssignments_TransactionServices_TransactionS~",
                        column: x => x.TransactionServiceId,
                        principalTable: "TransactionServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeId_Date",
                table: "Attendances",
                columns: new[] { "EmployeeId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeId_Date_TenantId",
                table: "Attendances",
                columns: new[] { "EmployeeId", "Date", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_TenantId",
                table: "Attendances",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Code_TenantId",
                table: "Branches",
                columns: new[] { "Code", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TenantId",
                table: "Branches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_CustomerId",
                table: "Cars",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_MakeId",
                table: "Cars",
                column: "MakeId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_ModelId",
                table: "Cars",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_PlateNumber",
                table: "Cars",
                column: "PlateNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_PlateNumber_TenantId",
                table: "Cars",
                columns: new[] { "PlateNumber", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cars_SizeId",
                table: "Cars",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_TenantId",
                table: "Cars",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_VehicleTypeId",
                table: "Cars",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email_TenantId",
                table: "Customers",
                columns: new[] { "Email", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_LastName_TenantId",
                table: "Customers",
                columns: new[] { "LastName", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId",
                table: "Customers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BranchId",
                table: "Employees",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BranchId_EmployeeType",
                table: "Employees",
                columns: new[] { "BranchId", "EmployeeType" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_LastName_TenantId",
                table: "Employees",
                columns: new[] { "LastName", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_TenantId",
                table: "Employees",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Makes_Name_TenantId",
                table: "Makes",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Makes_TenantId",
                table: "Makes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Merchandise_CategoryId",
                table: "Merchandise",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Merchandise_Sku_TenantId",
                table: "Merchandise",
                columns: new[] { "Sku", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Merchandise_TenantId",
                table: "Merchandise",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Merchandise_TenantId_StockQuantity",
                table: "Merchandise",
                columns: new[] { "TenantId", "StockQuantity" });

            migrationBuilder.CreateIndex(
                name: "IX_MerchandiseCategories_Name_TenantId",
                table: "MerchandiseCategories",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MerchandiseCategories_TenantId",
                table: "MerchandiseCategories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Models_MakeId",
                table: "Models",
                column: "MakeId");

            migrationBuilder.CreateIndex(
                name: "IX_Models_Name_MakeId",
                table: "Models",
                columns: new[] { "Name", "MakeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Models_TenantId",
                table: "Models",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageCommissions_PackageId",
                table: "PackageCommissions",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageCommissions_PackageId_VehicleTypeId_SizeId",
                table: "PackageCommissions",
                columns: new[] { "PackageId", "VehicleTypeId", "SizeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageCommissions_SizeId",
                table: "PackageCommissions",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageCommissions_TenantId",
                table: "PackageCommissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageCommissions_VehicleTypeId",
                table: "PackageCommissions",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEmployeeAssignments_EmployeeId",
                table: "PackageEmployeeAssignments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEmployeeAssignments_TenantId",
                table: "PackageEmployeeAssignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEmployeeAssignments_TransactionPackageId_EmployeeId",
                table: "PackageEmployeeAssignments",
                columns: new[] { "TransactionPackageId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackagePricing_PackageId",
                table: "PackagePricing",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePricing_PackageId_VehicleTypeId_SizeId",
                table: "PackagePricing",
                columns: new[] { "PackageId", "VehicleTypeId", "SizeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackagePricing_SizeId",
                table: "PackagePricing",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePricing_TenantId",
                table: "PackagePricing",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePricing_VehicleTypeId",
                table: "PackagePricing",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageServices_PackageId_ServiceId",
                table: "PackageServices",
                columns: new[] { "PackageId", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageServices_ServiceId",
                table: "PackageServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageServices_TenantId",
                table: "PackageServices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantId",
                table: "Payments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionId",
                table: "Payments",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntries_EmployeeId",
                table: "PayrollEntries",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntries_EmployeeId_PayrollPeriodId",
                table: "PayrollEntries",
                columns: new[] { "EmployeeId", "PayrollPeriodId" });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntries_PayrollPeriodId_EmployeeId",
                table: "PayrollEntries",
                columns: new[] { "PayrollPeriodId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntries_TenantId",
                table: "PayrollEntries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_TenantId",
                table: "PayrollPeriods",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_TenantId_Status",
                table: "PayrollPeriods",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_TenantId_Year_CutOffWeek",
                table: "PayrollPeriods",
                columns: new[] { "TenantId", "Year", "CutOffWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PricingModifiers_BranchId",
                table: "PricingModifiers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingModifiers_TenantId",
                table: "PricingModifiers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingModifiers_TenantId_Type",
                table: "PricingModifiers",
                columns: new[] { "TenantId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_BranchId",
                table: "QueueEntries",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_BranchId_QueueNumber_TenantId",
                table: "QueueEntries",
                columns: new[] { "BranchId", "QueueNumber", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_BranchId_Status_CreatedAt",
                table: "QueueEntries",
                columns: new[] { "BranchId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_CarId",
                table: "QueueEntries",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_CustomerId",
                table: "QueueEntries",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_TenantId",
                table: "QueueEntries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_TenantId_Status",
                table: "QueueEntries",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_TransactionId",
                table: "QueueEntries",
                column: "TransactionId",
                unique: true,
                filter: "\"TransactionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_Name_TenantId",
                table: "ServiceCategories",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_TenantId",
                table: "ServiceCategories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCommissions_ServiceId",
                table: "ServiceCommissions",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCommissions_ServiceId_VehicleTypeId_SizeId",
                table: "ServiceCommissions",
                columns: new[] { "ServiceId", "VehicleTypeId", "SizeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCommissions_SizeId",
                table: "ServiceCommissions",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCommissions_TenantId",
                table: "ServiceCommissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCommissions_VehicleTypeId",
                table: "ServiceCommissions",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEmployeeAssignments_EmployeeId",
                table: "ServiceEmployeeAssignments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEmployeeAssignments_TenantId",
                table: "ServiceEmployeeAssignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEmployeeAssignments_TransactionServiceId_EmployeeId",
                table: "ServiceEmployeeAssignments",
                columns: new[] { "TransactionServiceId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_Name_TenantId",
                table: "ServicePackages",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_TenantId",
                table: "ServicePackages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePricing_ServiceId",
                table: "ServicePricing",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePricing_ServiceId_VehicleTypeId_SizeId",
                table: "ServicePricing",
                columns: new[] { "ServiceId", "VehicleTypeId", "SizeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServicePricing_SizeId",
                table: "ServicePricing",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePricing_TenantId",
                table: "ServicePricing",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePricing_VehicleTypeId",
                table: "ServicePricing",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CategoryId",
                table: "Services",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Name_TenantId",
                table: "Services",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_TenantId",
                table: "Services",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Sizes_Name_TenantId",
                table: "Sizes",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sizes_TenantId",
                table: "Sizes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Email",
                table: "Tenants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionEmployees_EmployeeId_TransactionId",
                table: "TransactionEmployees",
                columns: new[] { "EmployeeId", "TransactionId" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionEmployees_TenantId",
                table: "TransactionEmployees",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionEmployees_TransactionId_EmployeeId",
                table: "TransactionEmployees",
                columns: new[] { "TransactionId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMerchandise_MerchandiseId",
                table: "TransactionMerchandise",
                column: "MerchandiseId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMerchandise_TenantId",
                table: "TransactionMerchandise",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMerchandise_TransactionId",
                table: "TransactionMerchandise",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPackages_PackageId",
                table: "TransactionPackages",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPackages_SizeId",
                table: "TransactionPackages",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPackages_TenantId",
                table: "TransactionPackages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPackages_TransactionId",
                table: "TransactionPackages",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPackages_VehicleTypeId",
                table: "TransactionPackages",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BranchId",
                table: "Transactions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BranchId_CreatedAt",
                table: "Transactions",
                columns: new[] { "BranchId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BranchId_Status",
                table: "Transactions",
                columns: new[] { "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CarId",
                table: "Transactions",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CashierId",
                table: "Transactions",
                column: "CashierId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CustomerId",
                table: "Transactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TenantId",
                table: "Transactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TenantId_TransactionNumber",
                table: "Transactions",
                columns: new[] { "TenantId", "TransactionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionServices_ServiceId",
                table: "TransactionServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionServices_SizeId",
                table: "TransactionServices",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionServices_TenantId",
                table: "TransactionServices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionServices_TransactionId",
                table: "TransactionServices",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionServices_VehicleTypeId",
                table: "TransactionServices",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClerkUserId",
                table: "Users",
                column: "ClerkUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTypes_Name_TenantId",
                table: "VehicleTypes",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTypes_TenantId",
                table: "VehicleTypes",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "PackageCommissions");

            migrationBuilder.DropTable(
                name: "PackageEmployeeAssignments");

            migrationBuilder.DropTable(
                name: "PackagePricing");

            migrationBuilder.DropTable(
                name: "PackageServices");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PayrollEntries");

            migrationBuilder.DropTable(
                name: "PricingModifiers");

            migrationBuilder.DropTable(
                name: "QueueEntries");

            migrationBuilder.DropTable(
                name: "ServiceCommissions");

            migrationBuilder.DropTable(
                name: "ServiceEmployeeAssignments");

            migrationBuilder.DropTable(
                name: "ServicePricing");

            migrationBuilder.DropTable(
                name: "TransactionEmployees");

            migrationBuilder.DropTable(
                name: "TransactionMerchandise");

            migrationBuilder.DropTable(
                name: "TransactionPackages");

            migrationBuilder.DropTable(
                name: "PayrollPeriods");

            migrationBuilder.DropTable(
                name: "TransactionServices");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Merchandise");

            migrationBuilder.DropTable(
                name: "ServicePackages");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "MerchandiseCategories");

            migrationBuilder.DropTable(
                name: "ServiceCategories");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Cars");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Models");

            migrationBuilder.DropTable(
                name: "Sizes");

            migrationBuilder.DropTable(
                name: "VehicleTypes");

            migrationBuilder.DropTable(
                name: "Makes");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
