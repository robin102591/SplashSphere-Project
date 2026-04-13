namespace SplashSphere.Application.Features.Inventory;

// Supply Categories
public sealed record SupplyCategoryDto(string Id, string Name, string? Description, bool IsActive);

// Supply Items
public sealed record SupplyItemDto(
    string Id, string BranchId, string BranchName, string? CategoryId, string? CategoryName,
    string Name, string? Description, string Unit, decimal CurrentStock, decimal? ReorderLevel,
    decimal AverageUnitCost, bool IsActive, bool IsLowStock, DateTime CreatedAt);

public sealed record SupplyItemDetailDto(
    string Id, string BranchId, string BranchName, string? CategoryId, string? CategoryName,
    string Name, string? Description, string Unit, decimal CurrentStock, decimal? ReorderLevel,
    decimal AverageUnitCost, bool IsActive, bool IsLowStock, DateTime CreatedAt,
    IReadOnlyList<StockMovementDto> RecentMovements);

// Stock Movements
public sealed record StockMovementDto(
    string Id, string BranchName, string ItemName, string Type,
    decimal Quantity, decimal? UnitCost, decimal? TotalCost,
    string? Reference, string? Notes, string? PerformedBy, DateTime MovementDate);

// Service Supply Usage
public sealed record ServiceSupplyUsageDto(
    string SupplyItemId, string SupplyItemName, string Unit,
    IReadOnlyList<SizeUsageDto> SizeUsages);

public sealed record SizeUsageDto(string? SizeId, string? SizeName, decimal QuantityPerUse);

public sealed record ServiceCostBreakdownDto(
    string ServiceName, decimal BasePrice,
    IReadOnlyList<SizeCostDto> SizeCosts);

public sealed record SizeCostDto(
    string SizeId, string SizeName,
    decimal ServicePrice, decimal SupplyCost, decimal EstimatedCommission, decimal GrossMargin, decimal MarginPercent,
    IReadOnlyList<SupplyCostLineDto> SupplyCostLines);

public sealed record SupplyCostLineDto(string SupplyName, string Unit, decimal QuantityPerUse, decimal UnitCost, decimal LineCost);

// Suppliers
public sealed record SupplierDto(string Id, string Name, string? ContactPerson, string? Phone, string? Email, string? Address, bool IsActive);

// Purchase Orders
public sealed record PurchaseOrderDto(
    string Id, string PoNumber, string SupplierName, string BranchName,
    string Status, decimal TotalAmount, DateTime? OrderDate, DateTime? ExpectedDeliveryDate, DateTime CreatedAt);

public sealed record PurchaseOrderDetailDto(
    string Id, string PoNumber, string SupplierId, string SupplierName, string BranchId, string BranchName,
    string Status, decimal TotalAmount, string? Notes, DateTime? OrderDate, DateTime? ExpectedDeliveryDate, DateTime CreatedAt,
    IReadOnlyList<PurchaseOrderLineDto> Lines);

public sealed record PurchaseOrderLineDto(
    string Id, string ItemName, string? SupplyItemId, string? MerchandiseId,
    decimal Quantity, decimal ReceivedQuantity, decimal UnitCost, decimal TotalCost);

// Equipment
public sealed record EquipmentDto(
    string Id, string BranchName, string Name, string? Brand, string? Model,
    string? SerialNumber, string Status, string? Location, bool IsActive,
    DateTime? LastMaintenanceDate, DateTime? NextMaintenanceDue, DateTime CreatedAt);

public sealed record EquipmentDetailDto(
    string Id, string BranchId, string BranchName, string Name, string? Brand, string? Model,
    string? SerialNumber, string Status, DateTime? PurchaseDate, decimal? PurchaseCost,
    DateTime? WarrantyExpiry, string? Location, string? Notes, bool IsActive, DateTime CreatedAt,
    IReadOnlyList<MaintenanceLogDto> MaintenanceLogs);

public sealed record MaintenanceLogDto(
    string Id, string Type, string Description, decimal? Cost, string? PerformedBy,
    DateTime PerformedDate, DateTime? NextDueDate, int? NextDueHours, string? Notes);

// ── Inventory Reports ────────────────────────────────────────────────────────

// Inventory Summary Report
public sealed record InventorySummaryDto(
    int TotalSupplyItems,
    int LowStockCount,
    int OutOfStockCount,
    decimal TotalStockValue,
    IReadOnlyList<LowStockItemDto> LowStockItems);

public sealed record LowStockItemDto(
    string Id, string Name, string Unit, string BranchName,
    decimal CurrentStock, decimal? ReorderLevel, decimal AverageUnitCost);

// Supply Usage Trend
public sealed record SupplyUsageTrendDto(
    DateOnly From, DateOnly To,
    IReadOnlyList<UsageTrendCategoryDto> Categories);

public sealed record UsageTrendCategoryDto(
    string CategoryName,
    IReadOnlyList<UsageTrendPointDto> DataPoints);

public sealed record UsageTrendPointDto(DateOnly Date, decimal TotalQuantity, decimal TotalCost);

// Equipment Maintenance Report
public sealed record EquipmentMaintenanceReportDto(
    int TotalEquipment,
    int NeedsMaintenanceCount,
    int UnderRepairCount,
    decimal TotalMaintenanceCostThisMonth,
    IReadOnlyList<MaintenanceDueItemDto> UpcomingMaintenance,
    IReadOnlyList<MaintenanceDueItemDto> OverdueMaintenance);

public sealed record MaintenanceDueItemDto(
    string EquipmentId, string EquipmentName, string BranchName,
    string? LastMaintenanceDescription, DateTime? NextDueDate, int DaysUntilDue);

// Purchase History
public sealed record PurchaseHistoryDto(
    DateOnly From, DateOnly To,
    decimal TotalSpending,
    IReadOnlyList<PurchaseBySupplierDto> BySupplier,
    IReadOnlyList<PurchaseByCategoryDto> ByCategory);

public sealed record PurchaseBySupplierDto(string SupplierName, decimal Amount, int OrderCount);
public sealed record PurchaseByCategoryDto(string CategoryName, decimal Amount);
