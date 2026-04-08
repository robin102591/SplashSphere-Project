# SplashSphere — Inventory Module Feature Spec

> **Phase:** 18 (after Franchise). Can be built independently — no dependency on Phases 16 or 17.
> **Plan gating:** Available on all plans (basic inventory). Advanced features (cost-per-wash analytics, purchase orders) gated to Growth+.

---

## Why This Matters

In a Philippine manual car wash, inventory is a constant headache:

- **Chemicals run out mid-day** — soap, shampoo, wax, tire black. When you're out, you can't serve customers. But ordering too much ties up cash.
- **Towels and chamois disappear** — microfiber towels, drying chamois, and wash mitts wear out, get lost, or walk out with employees. No one tracks how many you have.
- **Merchandise margins are invisible** — air fresheners, dash cleaners, and accessories are sold at POS, but the owner doesn't know the markup or when to reorder.
- **Equipment breaks without warning** — pressure washers, vacuums, and water pumps need maintenance. Without a schedule, they break on your busiest Saturday.
- **No cost-per-wash visibility** — how much soap, water, and chemicals does each wash actually consume? Without this, pricing is guesswork.

SplashSphere already tracks merchandise (the `Merchandise` entity exists for products sold at POS). This module expands inventory into three distinct categories and adds supply tracking, stock movements, purchase orders, and equipment maintenance logging.

---

## Inventory Categories

### 1. Merchandise (Already Exists — Enhance)

Products **sold to customers** at POS. Already in the system via the `Merchandise` entity.

**Examples:**
- Air fresheners (various scents)
- Dash cleaners / interior sprays
- Microfiber cloths (retail)
- Car accessories (phone holders, tissue boxes)
- Bottled water / drinks (if the shop sells them)

**What exists:** name, SKU, cost, price, inventoryCount, lowStockThreshold, category.

**What to add:** Purchase tracking (when stock was bought, from which supplier, at what cost), stock movement history (sale, restock, adjustment, damage), and COGS integration with the P&L report.

### 2. Supplies (New — Consumables Used in Operations)

Items **consumed during service delivery** — not sold directly to customers. These are operational costs.

**Typical Philippine car wash supplies:**

| Category | Items |
|---|---|
| **Cleaning Chemicals** | Car wash shampoo/soap (concentrated), foam soap, pre-soak cleaner, wheel & tire cleaner, glass cleaner, engine degreaser, bug & tar remover, interior cleaner, leather conditioner |
| **Wax & Polish** | Spray wax, paste wax, carnauba wax, polish compound, rubbing compound, ceramic coating solution, sealant |
| **Tire & Trim** | Tire black / tire dressing, vinyl & rubber protectant, trim restorer |
| **Towels & Cloths** | Microfiber towels (drying), microfiber towels (detailing), chamois/shammy, wash mitts, applicator pads, foam pads |
| **Brushes & Tools** | Wheel brushes, detail brushes, clay bars, sponges, squeegees |
| **Water & Utilities** | Water (metered usage if tracked), electricity (optional) |
| **Miscellaneous** | Masking tape, plastic bags, paper towels, garbage bags, safety gloves |

**Key difference from merchandise:** Supplies are consumed during service, not sold. Their cost contributes to COGS (Cost of Goods Sold) in the P&L report.

### 3. Equipment (New — Assets with Maintenance Schedules)

Durable assets that need **maintenance tracking** rather than inventory counting.

**Typical Philippine car wash equipment:**

| Equipment | Maintenance Needs |
|---|---|
| **Pressure washer** | Oil change every 50 hours, nozzle replacement, hose inspection, pump seal check |
| **Vacuum cleaner** | Filter cleaning/replacement, motor inspection, hose check |
| **Water pump** | Seal inspection, impeller check, bearing lubrication |
| **Air compressor** | Drain moisture, check belts, oil change, filter replacement |
| **Water tank** | Cleaning, float valve inspection, leak check |
| **Foam gun / cannon** | Nozzle cleaning, seal replacement |
| **Buffer / polisher** | Pad replacement, cord inspection |
| **Blower / dryer** | Filter cleaning, motor check |

---

## Domain Models

### SupplyItem (New Entity)

```csharp
public sealed class SupplyItem : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public string Unit { get; set; } = string.Empty;         // liters, pieces, bottles, kg, packs
    public decimal CurrentStock { get; set; }
    public decimal ReorderLevel { get; set; }                 // Alert when stock falls below this
    public decimal UnitCost { get; set; }                     // Latest purchase cost per unit
    public decimal? AverageUnitCost { get; set; }             // Weighted average cost
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public SupplyCategory Category { get; set; } = null!;
    public List<StockMovement> StockMovements { get; set; } = [];
}
```

### SupplyCategory (New Entity)

```csharp
public sealed class SupplyCategory : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;          // Cleaning Chemicals, Towels & Cloths, etc.
    public string? Icon { get; set; }                          // Lucide icon name
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public List<SupplyItem> Items { get; set; } = [];
}
```

### StockMovement (New Entity — Shared by Supplies and Merchandise)

Tracks every change to stock levels. Audit trail for all inventory.

```csharp
public sealed class StockMovement : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string? SupplyItemId { get; set; }                 // For supply items
    public string? MerchandiseId { get; set; }                // For merchandise
    public MovementType Type { get; set; }
    public decimal Quantity { get; set; }                     // Positive = in, Negative = out
    public decimal UnitCost { get; set; }                     // Cost at time of movement
    public decimal TotalCost { get; set; }                    // Quantity × UnitCost
    public string? Reference { get; set; }                    // PO number, transaction ID, adjustment reason
    public string? Notes { get; set; }
    public string RecordedById { get; set; } = string.Empty;  // UserId
    public DateTime MovementDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public SupplyItem? SupplyItem { get; set; }
    public Merchandise? Merchandise { get; set; }
    public User RecordedBy { get; set; } = null!;
}

public enum MovementType
{
    PurchaseIn,       // Stock received from supplier
    UsageOut,         // Consumed during service (supplies only)
    SaleOut,          // Sold to customer at POS (merchandise only)
    AdjustmentIn,     // Manual increase (found stock, correction)
    AdjustmentOut,    // Manual decrease (damaged, expired, lost)
    TransferIn,       // Received from another branch
    TransferOut,      // Sent to another branch
    ReturnIn,         // Returned from customer
    WasteOut          // Written off (damaged, expired)
}
```

### PurchaseOrder (New Entity)

```csharp
public sealed class PurchaseOrder : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string PoNumber { get; set; } = string.Empty;      // Auto-generated: PO-2026-0001
    public string SupplierId { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public string? ReceivedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Supplier Supplier { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public List<PurchaseOrderLine> Lines { get; set; } = [];
}

public sealed class PurchaseOrderLine
{
    public string Id { get; set; } = string.Empty;
    public string PurchaseOrderId { get; set; } = string.Empty;
    public string? SupplyItemId { get; set; }
    public string? MerchandiseId { get; set; }
    public string ItemName { get; set; } = string.Empty;       // Denormalized for history
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
}

public enum PurchaseOrderStatus
{
    Draft,        // Being prepared
    Sent,         // Sent to supplier
    PartiallyReceived,
    Received,     // All items received
    Cancelled
}
```

### Supplier (New Entity)

```csharp
public sealed class Supplier : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public List<PurchaseOrder> PurchaseOrders { get; set; } = [];
}
```

### Equipment (New Entity)

```csharp
public sealed class Equipment : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;          // "Pressure Washer #1"
    public string? Brand { get; set; }                         // "Karcher"
    public string? Model { get; set; }                         // "HD 5/11 C"
    public string? SerialNumber { get; set; }
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Operational;
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchaseCost { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public string? Location { get; set; }                      // "Bay 1", "Detailing area"
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public List<MaintenanceLog> MaintenanceLogs { get; set; } = [];
}

public sealed class MaintenanceLog : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string EquipmentId { get; set; } = string.Empty;
    public MaintenanceType Type { get; set; }
    public string Description { get; set; } = string.Empty;    // "Oil change", "Nozzle replaced"
    public decimal? Cost { get; set; }                         // Repair/parts cost
    public string? PerformedBy { get; set; }                   // Technician name or employee
    public DateTime PerformedDate { get; set; }
    public DateTime? NextDueDate { get; set; }                 // When next maintenance is due
    public int? NextDueHours { get; set; }                     // Or after X operating hours
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Equipment Equipment { get; set; } = null!;
}

public enum EquipmentStatus { Operational, NeedsMaintenance, UnderRepair, Retired }
public enum MaintenanceType { Preventive, Corrective, Inspection, PartReplacement }
```

### ServiceSupplyUsage (New — Links Services to Supply Consumption by Vehicle Size)

Defines how much of each supply is consumed per service, **varying by vehicle size**. This mirrors the pricing matrix pattern — a Large SUV consumes more soap, water, and towel time than a Small Sedan.

The matrix works like this:

```
Service: Basic Wash
┌──────────────────┬─────────┬─────────┬─────────┬──────────┐
│ Supply Item      │ Small   │ Medium  │ Large   │ XL       │
├──────────────────┼─────────┼─────────┼─────────┼──────────┤
│ Car Wash Soap    │ 0.03 L  │ 0.05 L  │ 0.08 L  │ 0.12 L   │
│ Wax Spray        │ 0.01 L  │ 0.02 L  │ 0.03 L  │ 0.04 L   │
│ Microfiber Towel │ 1 use   │ 1 use   │ 2 uses  │ 2 uses   │
│ Water            │ 30 L    │ 50 L    │ 80 L    │ 120 L    │
└──────────────────┴─────────┴─────────┴─────────┴──────────┘
```

When a transaction is completed for a "Basic Wash on a Large SUV," the system looks up the Large column and deducts 0.08L soap, 0.03L wax, 2 towel uses, and 80L water.

```csharp
public sealed class ServiceSupplyUsage
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string SupplyItemId { get; set; } = string.Empty;
    public string SizeId { get; set; } = string.Empty;         // Vehicle size determines consumption
    public decimal QuantityPerUse { get; set; }                // Amount consumed per wash for this size
    public string? Notes { get; set; }

    public Service Service { get; set; } = null!;
    public SupplyItem SupplyItem { get; set; } = null!;
    public Size Size { get; set; } = null!;
}
```

**Fallback logic:** If no size-specific entry exists, the system looks for an entry where `SizeId` is null (the "default" amount). If neither exists, no auto-deduction happens for that supply — the owner uses manual recording instead.

**Why size and not vehicle type?** In practice, a Sedan and a Hatchback use roughly the same amount of soap — it's the physical size of the vehicle that determines consumption, not the body type. A Large Sedan (like a Camry) uses similar supplies to a Large SUV. This keeps the matrix manageable — you configure per size (4 columns), not per vehicle type × size (potentially 20+ combinations). This matches how Philippine car wash attendants think: "malaki 'yan, more soap" (that's big, more soap).

---

## Business Rules

### Stock Management

1. Every stock change creates a `StockMovement` record — no silent updates to quantities.
2. `CurrentStock` on `SupplyItem` and `inventoryCount` on `Merchandise` are always the sum of all movements. Recalculable.
3. When stock falls below `ReorderLevel`, the item is flagged as "Low Stock" in the dashboard.
4. Negative stock is allowed (backfill scenario) but triggers a warning.
5. Stock is tracked **per branch** — each branch has its own supply quantities.

### Supply Usage Tracking

6. When a transaction is completed, if `ServiceSupplyUsage` records exist for the services performed, the system auto-deducts supplies based on the vehicle's size:
   - For each `TransactionService` in the transaction:
     a. Get the `Car.SizeId` from the transaction (the vehicle being washed)
     b. Look up `ServiceSupplyUsage` entries matching `ServiceId` + `SizeId`
     c. If no size-specific entry exists, fall back to entries where `SizeId` is null (default)
     d. If no entries at all, skip (no auto-deduction for this service)
     e. For each matched entry: create `StockMovement` (type = `UsageOut`, quantity = `QuantityPerUse × TransactionService.Quantity`)
     f. Deduct from `SupplyItem.CurrentStock`
   - For package transactions (`TransactionPackage`): apply the same logic per included service
7. This auto-deduction is **optional** — the tenant configures it per service. If not configured, supplies are deducted manually via stock adjustments.
8. Manual usage recording is always available: "We used 2 bottles of soap today" → creates a UsageOut movement.
9. If a transaction has no car linked (walk-in without vehicle lookup), use the default (null SizeId) entries.

### Merchandise Sales Integration

9. When a `TransactionMerchandise` is created (merchandise sold at POS), the system auto-creates a `StockMovement` (type = `SaleOut`) and decrements `Merchandise.inventoryCount`.
10. Merchandise cost tracks via `StockMovement.UnitCost` for accurate COGS in P&L reports.

### Purchase Orders

11. PO lifecycle: `Draft` → `Sent` → `Received` (or `PartiallyReceived` → `Received`).
12. On "Receive" action: for each PO line, create `StockMovement` (type = `PurchaseIn`), increment stock, update `UnitCost` on the item.
13. Partial receiving is supported — record what arrived, leave the PO in `PartiallyReceived`.
14. PO number auto-generates: `PO-{YYYY}-{Sequence}`.

### Equipment Maintenance

15. Equipment has a status: `Operational` → `NeedsMaintenance` → `UnderRepair` → `Operational`.
16. `MaintenanceLog.NextDueDate` triggers a dashboard alert when maintenance is due.
17. Hangfire job checks daily for equipment with `NextDueDate <= today` and sets status to `NeedsMaintenance`.
18. Maintenance costs feed into the Expense Tracking module (Phase 15) if integrated.

### Cost-Per-Wash Calculation

The cost varies by vehicle size because supply consumption differs:

```
Service: Basic Wash — Cost Breakdown by Size

┌──────────────────┬─────────────┬─────────────┬─────────────┬─────────────┐
│ Supply           │ Small       │ Medium      │ Large       │ XL          │
├──────────────────┼─────────────┼─────────────┼─────────────┼─────────────┤
│ Soap (₱180/L)    │ 0.03L = ₱5  │ 0.05L = ₱9  │ 0.08L = ₱14 │ 0.12L = ₱22 │
│ Wax (₱250/L)     │ 0.01L = ₱3  │ 0.02L = ₱5  │ 0.03L = ₱8  │ 0.04L = ₱10 │
│ Towel (₱2/use)   │ 1 = ₱2      │ 1 = ₱2      │ 2 = ₱4      │ 2 = ₱4      │
│ Water (₱0.05/L)  │ 30L = ₱2    │ 50L = ₱3    │ 80L = ₱4    │ 120L = ₱6   │
├──────────────────┼─────────────┼─────────────┼─────────────┼─────────────┤
│ Supply Cost      │ ₱12.00      │ ₱19.00      │ ₱30.00      │ ₱42.00      │
│ Service Price    │ ₱180.00     │ ₱220.00     │ ₱280.00     │ ₱350.00     │
│ Commission (×3)  │ ₱30.00      │ ₱40.00      │ ₱48.00      │ ₱60.00      │
│ Gross Margin     │ ₱138 (77%)  │ ₱161 (73%)  │ ₱202 (72%)  │ ₱248 (71%)  │
└──────────────────┴─────────────┴─────────────┴─────────────┴─────────────┘
```

This tells the owner: "Your Large SUV wash makes ₱202 after supplies and commissions, but your XL Van only makes ₱248 despite costing ₱42 in supplies — maybe your XL pricing needs to go up."

The cost-per-wash report shows this matrix for every service, pulling live data from `ServiceSupplyUsage` quantities × `SupplyItem.AverageUnitCost`.

---

## Default Supply Categories (Seeded)

```
Cleaning Chemicals
Wax & Polish
Tire & Trim Products
Towels & Cloths
Brushes & Tools
Water & Utilities
Packaging & Miscellaneous
```

---

## API Endpoints

### Supply Items

| Method | Route | Description |
|---|---|---|
| `GET` | `/supplies` | List supply items (filter by category, branch, low stock) |
| `POST` | `/supplies` | Create supply item |
| `GET` | `/supplies/{id}` | Supply item detail with movement history |
| `PUT` | `/supplies/{id}` | Update supply item |
| `DELETE` | `/supplies/{id}` | Soft delete |
| `GET` | `/supply-categories` | List supply categories |
| `POST` | `/supply-categories` | Create category |

### Stock Movements

| Method | Route | Description |
|---|---|---|
| `POST` | `/stock-movements` | Record a stock movement (usage, adjustment, transfer) |
| `GET` | `/stock-movements` | List movements (filter by item, type, branch, date range) |
| `POST` | `/stock-movements/bulk-usage` | Record daily usage for multiple supplies at once |

### Purchase Orders

| Method | Route | Description |
|---|---|---|
| `GET` | `/purchase-orders` | List POs (filter by status, supplier, branch, date) |
| `POST` | `/purchase-orders` | Create PO |
| `GET` | `/purchase-orders/{id}` | PO detail with lines |
| `PUT` | `/purchase-orders/{id}` | Update PO (only in Draft) |
| `PATCH` | `/purchase-orders/{id}/send` | Mark as Sent |
| `POST` | `/purchase-orders/{id}/receive` | Receive items (partial or full) |
| `PATCH` | `/purchase-orders/{id}/cancel` | Cancel PO |

### Suppliers

| Method | Route | Description |
|---|---|---|
| `GET` | `/suppliers` | List suppliers |
| `POST` | `/suppliers` | Create supplier |
| `PUT` | `/suppliers/{id}` | Update supplier |

### Equipment

| Method | Route | Description |
|---|---|---|
| `GET` | `/equipment` | List equipment (filter by branch, status) |
| `POST` | `/equipment` | Register equipment |
| `GET` | `/equipment/{id}` | Equipment detail with maintenance history |
| `PUT` | `/equipment/{id}` | Update equipment |
| `POST` | `/equipment/{id}/maintenance` | Log a maintenance activity |
| `PATCH` | `/equipment/{id}/status` | Update equipment status |

### Service Supply Usage

| Method | Route | Description |
|---|---|---|
| `GET` | `/services/{id}/supply-usage` | Get supply usage matrix for a service (all sizes) |
| `PUT` | `/services/{id}/supply-usage` | Set/update the full supply usage matrix (list of { supplyItemId, sizeId, quantityPerUse }) |
| `GET` | `/services/{id}/supply-usage/{sizeId}` | Get usage for a specific size |
| `GET` | `/services/{id}/cost-breakdown` | Get cost-per-wash breakdown by size (calculated from usage × unit costs) |

### Reports

| Method | Route | Description |
|---|---|---|
| `GET` | `/reports/inventory-summary` | Current stock levels, value, low stock alerts |
| `GET` | `/reports/cost-per-wash` | Cost breakdown per service |
| `GET` | `/reports/supply-usage` | Supply consumption over time (daily/weekly/monthly) |
| `GET` | `/reports/equipment-maintenance` | Upcoming and overdue maintenance |
| `GET` | `/reports/purchase-history` | Spending by supplier, category, period |

---

## Frontend Pages

### Admin Dashboard — Inventory Section

Add "Inventory" nav group to sidebar with sub-items:

```
Inventory
├── Supplies
├── Merchandise (existing, enhanced)
├── Equipment
├── Purchase Orders
├── Suppliers
```

### Supply Pages

| Route | Page |
|---|---|
| `/supplies` | Supply list: filters (category, branch, stock status). Table: name, category, current stock, unit, reorder level, unit cost, status badge (OK / Low / Out). Low stock rows highlighted amber. Out-of-stock rows highlighted red. |
| `/supplies/new` | Create supply form: name, category, unit, initial stock, reorder level, unit cost, branch |
| `/supplies/[id]` | Supply detail: info card, stock level gauge, movement history timeline. "Record Usage" and "Restock" quick action buttons. Cost trend chart (unit cost over time). |

### Stock Movement Dialog

Quick-action dialogs accessible from supply detail or supply list:

**Record Usage:** "How much was used today?"
- Quantity input, reason (preset: "Daily operations", "Extra service", "Waste"), notes
- Creates `UsageOut` movement

**Restock:** "How much did you receive?"
- Quantity, unit cost, supplier (optional), PO reference (optional)
- Creates `PurchaseIn` movement, updates unit cost

**Adjust Stock:** "Correct the count"
- New quantity (system calculates the adjustment needed), reason (preset: "Physical count", "Damage", "Expired", "Found")
- Creates `AdjustmentIn` or `AdjustmentOut` movement

### Purchase Order Pages

| Route | Page |
|---|---|
| `/purchase-orders` | PO list: status badges, supplier, total, date. "New PO" button. |
| `/purchase-orders/new` | Create PO: select supplier, add line items (search supply/merchandise), quantities, costs. Auto-calculates total. |
| `/purchase-orders/[id]` | PO detail: line items with ordered vs received quantities. "Receive Items" button opens a form to enter received quantities. Status timeline. |

### Equipment Pages

| Route | Page |
|---|---|
| `/equipment` | Equipment list: name, branch, status badge, last maintenance date, next due date. "Needs Maintenance" items highlighted amber. |
| `/equipment/new` | Register equipment: name, brand, model, serial, branch, purchase date, cost, warranty expiry. |
| `/equipment/[id]` | Equipment detail: info card, maintenance history timeline, "Log Maintenance" button. Next maintenance due card with countdown. |

### Merchandise Enhancement

Update existing `/merchandise` pages:
- Add "Stock Movements" tab to merchandise detail page
- Add "Restock" button that creates a PurchaseIn movement
- Show cost vs price margin on the list page
- Low stock badge with pulse animation

### Inventory Dashboard Widget

Add to the admin dashboard (home page):

```
┌─ Inventory Alerts ──────────────────────────────┐
│ ⚠ 3 supplies low stock                          │
│   • Car Wash Soap — 2 liters remaining           │
│   • Microfiber Towels — 5 pieces remaining        │
│   • Tire Black — 0.5 liters remaining             │
│ 🔧 1 equipment needs maintenance                  │
│   • Pressure Washer #2 — oil change overdue        │
│                                    [View All →]    │
└─────────────────────────────────────────────────────┘
```

### POS Integration

On the POS transaction screen, when a service is completed:
- The system resolves the vehicle's size (e.g., "Large" from the Fortuner's Size record) and deducts the size-specific supply quantities
- Show a subtle notification: "Supplies deducted (Large): Soap -0.08L, Wax -0.03L, Towel ×2, Water -80L"
- If a supply is at zero or below reorder level after deduction, show a warning toast: "⚠ Car Wash Soap is running low (2L remaining)"
- If no vehicle is linked to the transaction (walk-in), default quantities are used and the notification says "(default)" instead of the size name

---

## Hangfire Background Jobs

| Job | Schedule | Description |
|---|---|---|
| `CheckLowStockAlerts` | Every 6 hours | Check all supplies and merchandise. If below reorder level, send notification (in-app + optional SMS to manager). |
| `CheckEquipmentMaintenance` | Daily 8 AM | Check all equipment. If `NextDueDate <= today`, set status to `NeedsMaintenance`, send alert. |
| `RecalculateAverageUnitCost` | Weekly Sunday | For each supply item, recalculate `AverageUnitCost` using weighted average from `PurchaseIn` movements. |

---

## P&L Integration

Update the existing Profit & Loss report (from Phase 15 Expense Tracking):

```
Revenue          = SUM(completed transactions)
─────────────────────────────────────────────────
COGS:
  Merchandise    = SUM(StockMovement.TotalCost WHERE type = SaleOut)
  Supply Usage   = SUM(StockMovement.TotalCost WHERE type = UsageOut)
  Total COGS     = Merchandise + Supply Usage
─────────────────────────────────────────────────
Gross Profit     = Revenue - COGS
─────────────────────────────────────────────────
Expenses:
  (from Expense Tracking module)
  Equipment Maintenance = SUM(MaintenanceLog.Cost) — optional integration
─────────────────────────────────────────────────
Net Profit       = Gross Profit - Total Expenses
```

This gives the owner a true profitability picture: "We made ₱32K in revenue, spent ₱3.2K on chemicals and ₱1.8K on towels, paid ₱8.9K in commissions, ₱2.1K on electricity, ₱800 on pressure washer repair — actual profit: ₱15.2K."

---

## Claude Code Prompts — Phase 18

### Prompt 18.1 — Inventory Domain + Infrastructure

```
Add the Inventory Module to SplashSphere:

Domain/Entities/:
- SupplyItem, SupplyCategory, StockMovement, PurchaseOrder, PurchaseOrderLine,
  Supplier, Equipment, MaintenanceLog, ServiceSupplyUsage

Domain/Enums/:
- MovementType, PurchaseOrderStatus, EquipmentStatus, MaintenanceType

Update Tenant: add navigation lists for SupplyItems, SupplyCategories, Suppliers,
Equipment, PurchaseOrders.
Update Branch: add navigation lists for SupplyItems, Equipment.
Update Service: add List<ServiceSupplyUsage> navigation.
Update Size: add List<ServiceSupplyUsage> navigation.
Update Merchandise: add List<StockMovement> navigation.

Infrastructure/Persistence/Configurations/:
- EF configurations for all new entities
- StockMovement: index on [tenantId, supplyItemId, movementDate],
  [tenantId, merchandiseId, movementDate]
- PurchaseOrder: unique on [tenantId, poNumber]
- ServiceSupplyUsage: unique on [serviceId, supplyItemId, sizeId]
  Add navigation to Size entity.
  SizeId is nullable — null means "default for all sizes" (fallback).
- Equipment: index on [tenantId, branchId, status]
- Apply tenant global filter on all new entities

Seed default SupplyCategories during DataSeeder.
Migration: "AddInventoryModule"
```

### Prompt 18.2 — Inventory Application Layer (Supplies + Stock)

```
Build the Inventory CQRS features:

Application/Features/Inventory/Supplies/:
- CreateSupplyItemCommand, UpdateSupplyItemCommand, DeleteSupplyItemCommand
- GetSuppliesQuery (filter by category, branch, stock status, paginated)
- GetSupplyByIdQuery (includes recent movements)
- GetSupplyCategoriesQuery, CreateSupplyCategoryCommand

Application/Features/Inventory/StockMovements/:
- RecordStockMovementCommand (itemType, itemId, type, quantity, unitCost, reference, notes)
  Validates: item exists, quantity > 0 for "in" types
  Updates CurrentStock on the item
  Recalculates AverageUnitCost for PurchaseIn movements
- RecordBulkUsageCommand (list of { supplyItemId, quantity })
  For end-of-day bulk usage recording
- GetStockMovementsQuery (filter by item, type, date range)

Application/Features/Inventory/ServiceUsage/:
- UpdateServiceSupplyUsageCommand (serviceId, list of { supplyItemId, sizeId, quantityPerUse })
  This accepts the full matrix: for each supply × size combination, a quantity.
  sizeId = null means "default for any size not explicitly configured."
- GetServiceSupplyUsageQuery (serviceId) → returns the full matrix grouped by supply item,
  with columns for each size
- GetServiceCostBreakdownQuery (serviceId) → calculated cost per size using 
  QuantityPerUse × SupplyItem.AverageUnitCost, plus service price and commission for margin

CRITICAL: Update CreateTransactionCommandHandler:
After step 9 (transaction saved), if ServiceSupplyUsage records exist for the services:
1. For each TransactionService in the transaction:
   a. Get the vehicle's SizeId from TransactionService.Car.SizeId
   b. Query ServiceSupplyUsage WHERE ServiceId = service AND 
      (SizeId = vehicle's sizeId OR SizeId IS NULL)
      Prefer the size-specific match; fall back to null (default)
   c. For each matched entry: 
      quantity = QuantityPerUse × TransactionService.Quantity
      Create StockMovement (UsageOut, quantity, linked to branch)
      Deduct from SupplyItem.CurrentStock
2. For TransactionPackage items: resolve included services, apply same logic
3. If a transaction has no Car (walk-in), use the null/default entries only
4. If any supply falls below ReorderLevel after deduction, publish LowStockEvent

Endpoints: SupplyEndpoints.cs, StockMovementEndpoints.cs, ServiceUsageEndpoints.cs
```

### Prompt 18.3 — Purchase Orders + Suppliers + Equipment

```
Build:

Application/Features/Inventory/PurchaseOrders/:
- CreatePurchaseOrderCommand (supplierId, branchId, lines[])
  Auto-generate PO number: PO-{YYYY}-{Sequence}
- UpdatePurchaseOrderCommand (only Draft status)
- SendPurchaseOrderCommand → status = Sent
- ReceivePurchaseOrderCommand (lines with receivedQuantity[])
  For each line: create PurchaseIn StockMovement, update item stock and cost
  If all lines fully received → status = Received
  If partial → status = PartiallyReceived
- CancelPurchaseOrderCommand
- GetPurchaseOrdersQuery, GetPurchaseOrderByIdQuery

Application/Features/Inventory/Suppliers/:
- CRUD for Supplier
- GetSuppliersQuery

Application/Features/Inventory/Equipment/:
- RegisterEquipmentCommand, UpdateEquipmentCommand
- LogMaintenanceCommand (equipmentId, type, description, cost, performedDate, nextDueDate)
  Updates equipment status to Operational after maintenance
- UpdateEquipmentStatusCommand
- GetEquipmentQuery (filter by branch, status)
- GetEquipmentByIdQuery (includes maintenance history)
- GetMaintenanceDueQuery → equipment with nextDueDate approaching

Background jobs:
- CheckLowStockJob (every 6 hours): check all supplies + merchandise, flag low stock
- CheckMaintenanceDueJob (daily 8AM): check equipment, set NeedsMaintenance status

Application/Features/Reports/ (add or update):
- GetInventorySummaryQuery → stock levels, total value, low stock count, out of stock count
- GetCostPerWashQuery → per service, per size: supply costs breakdown, service price, 
  commission estimate, gross margin. Returns a matrix matching the cost-per-wash table
  in the spec. Pulls QuantityPerUse from ServiceSupplyUsage × AverageUnitCost from SupplyItem.
- GetSupplyUsageTrendQuery → usage by category over time
- GetEquipmentMaintenanceReportQuery → upcoming, overdue, cost history
- UPDATE GetProfitLossReportQuery → include COGS from StockMovements

Endpoints: PurchaseOrderEndpoints.cs, SupplierEndpoints.cs, EquipmentEndpoints.cs
Update ReportEndpoints.cs with new report routes.
```

### Prompt 18.4 — Inventory Frontend (Admin)

```
Build inventory admin pages:

1. Add "Inventory" nav group with: Supplies, Merchandise (existing), Equipment,
   Purchase Orders, Suppliers

2. /supplies — list page:
   Filters: category dropdown, branch, stock status (All / Low Stock / Out of Stock)
   Table: Name, Category, Stock (with colored bar: green >50%, amber >20%, red ≤20%),
   Unit, Reorder Level, Unit Cost, Status badge
   Quick actions: "Record Usage", "Restock" open dialog modals

3. /supplies/new — create form
4. /supplies/[id] — detail page with tabs:
   - Details: info card, stock level gauge
   - Movements: timeline of all stock changes with type badges
   - Usage Config: which services use this supply and how much
   - Cost History: unit cost trend chart

5. /equipment — list page with status badges and maintenance due indicators
6. /equipment/new — register form
7. /equipment/[id] — detail with maintenance log timeline, "Log Maintenance" button

8. /purchase-orders — list page with status badges
9. /purchase-orders/new — create PO form: supplier picker, add lines (search items),
   quantities, costs
10. /purchase-orders/[id] — detail with "Receive" button (enter received quantities)

11. /suppliers — simple CRUD list/form

12. Update existing /merchandise/[id] — add "Stock Movements" tab

13. Update /reports — add:
    - "Inventory Summary" report card
    - "Cost per Wash" report with per-service breakdown table
    - "Supply Usage" chart (daily/weekly consumption)

14. Update admin dashboard:
    Add "Inventory Alerts" widget showing low stock items and equipment maintenance due

15. /services/[id] — add "Supply Usage" tab:
    Matrix editor matching the pricing matrix pattern:
    Rows = supply items, Columns = vehicle sizes (Small, Medium, Large, XL)
    Each cell = quantity consumed per wash for that size
    
    ┌──────────────────┬─────────┬─────────┬─────────┬──────────┐
    │ Supply Item      │ Small   │ Medium  │ Large   │ XL       │
    ├──────────────────┼─────────┼─────────┼─────────┼──────────┤
    │ Car Wash Soap    │ [0.03]  │ [0.05]  │ [0.08]  │ [0.12]   │
    │ Wax Spray        │ [0.01]  │ [0.02]  │ [0.03]  │ [0.04]   │
    │ Microfiber Towel │ [1   ]  │ [1   ]  │ [2   ]  │ [2   ]   │
    │ Water            │ [30  ]  │ [50  ]  │ [80  ]  │ [120 ]   │
    └──────────────────┴─────────┴─────────┴─────────┴──────────┘
    
    "Add Supply" button adds a new row.
    Below the matrix: auto-calculated cost summary per size using live unit costs.
    Uses the same PricingMatrixEditor component pattern from UI_UX_ADDENDUM.
    Empty cells mean "no auto-deduction for this supply/size combo."
```

---

## Subscription Plan Gating

| Feature | Starter | Growth | Enterprise |
|---|---|---|---|
| Merchandise tracking (existing) | ✓ | ✓ | ✓ |
| Basic supply tracking (CRUD, manual stock) | ✓ | ✓ | ✓ |
| Stock movement history | ✓ | ✓ | ✓ |
| Low stock alerts | ✓ | ✓ | ✓ |
| Purchase orders | — | ✓ | ✓ |
| Supplier management | — | ✓ | ✓ |
| Equipment & maintenance | — | ✓ | ✓ |
| Service supply usage (auto-deduction) | — | ✓ | ✓ |
| Cost-per-wash reports | — | ✓ | ✓ |
| Supply usage analytics | — | ✓ | ✓ |
| Branch-to-branch transfers | — | — | ✓ |

---

## Phase Summary

| Prompt | What | Layer |
|---|---|---|
| 18.1 | Domain entities, enums, EF configs, seed data, migration | Backend |
| 18.2 | Supply CRUD, stock movements, service usage config, transaction integration | Backend |
| 18.3 | Purchase orders, suppliers, equipment, maintenance, reports, background jobs | Backend |
| 18.4 | All inventory frontend pages, dashboard widget, POS integration | Frontend |

**Total: 4 prompts in Phase 18.**
