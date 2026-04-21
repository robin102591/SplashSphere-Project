using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext exposed to the Application layer.
/// Handlers inject this interface; the concrete <c>ApplicationDbContext</c> in
/// Infrastructure satisfies it. Using the interface keeps the Application layer
/// independent of any specific ORM implementation and makes handlers unit-testable.
/// <para>
/// <b>Tenant isolation:</b> all DbSets have global query filters applied in
/// <c>ApplicationDbContext.OnModelCreating</c>. Use <c>.IgnoreQueryFilters()</c>
/// only when cross-tenant access is explicitly required (e.g. webhook handlers,
/// user lookup before tenant context is populated).
/// </para>
/// </summary>
public interface IApplicationDbContext
{
    // ── EF Core infrastructure ────────────────────────────────────────────────
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // ── Tenant & identity ─────────────────────────────────────────────────────
    DbSet<Tenant> Tenants { get; }
    DbSet<Branch> Branches { get; }
    DbSet<User> Users { get; }

    // ── Vehicle master data ───────────────────────────────────────────────────
    DbSet<VehicleType> VehicleTypes { get; }
    DbSet<Size> Sizes { get; }
    DbSet<Make> Makes { get; }
    DbSet<Model> Models { get; }

    // ── Service catalogue ─────────────────────────────────────────────────────
    DbSet<ServiceCategory> ServiceCategories { get; }
    DbSet<Service> Services { get; }
    DbSet<ServicePricing> ServicePricings { get; }
    DbSet<ServiceCommission> ServiceCommissions { get; }

    // ── Package catalogue ─────────────────────────────────────────────────────
    DbSet<ServicePackage> ServicePackages { get; }
    DbSet<PackageService> PackageServices { get; }
    DbSet<PackagePricing> PackagePricings { get; }
    DbSet<PackageCommission> PackageCommissions { get; }

    // ── Pricing modifiers ─────────────────────────────────────────────────────
    DbSet<PricingModifier> PricingModifiers { get; }

    // ── Customers & vehicles ──────────────────────────────────────────────────
    DbSet<Customer> Customers { get; }
    DbSet<Car> Cars { get; }

    // ── Merchandise ───────────────────────────────────────────────────────────
    DbSet<MerchandiseCategory> MerchandiseCategories { get; }
    DbSet<Merchandise> Merchandise { get; }

    // ── Employees & payroll ───────────────────────────────────────────────────
    DbSet<Employee> Employees { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<PayrollPeriod> PayrollPeriods { get; }
    DbSet<PayrollEntry> PayrollEntries { get; }

    // ── Transactions ──────────────────────────────────────────────────────────
    DbSet<Transaction> Transactions { get; }
    DbSet<TransactionService> TransactionServices { get; }
    DbSet<TransactionPackage> TransactionPackages { get; }
    DbSet<TransactionMerchandise> TransactionMerchandise { get; }
    DbSet<TransactionEmployee> TransactionEmployees { get; }
    DbSet<Payment> Payments { get; }
    DbSet<ServiceEmployeeAssignment> ServiceEmployeeAssignments { get; }
    DbSet<PackageEmployeeAssignment> PackageEmployeeAssignments { get; }

    // ── Queue ─────────────────────────────────────────────────────────────────
    DbSet<QueueEntry> QueueEntries { get; }

    // ── Cashier shifts ────────────────────────────────────────────────────────
    DbSet<CashierShift> CashierShifts { get; }
    DbSet<CashMovement> CashMovements { get; }
    DbSet<ShiftDenomination> ShiftDenominations { get; }
    DbSet<ShiftPaymentSummary> ShiftPaymentSummaries { get; }
    DbSet<ShiftSettings> ShiftSettings { get; }

    // ── Payroll adjustment templates, adjustments & settings ────────────────
    DbSet<PayrollAdjustmentTemplate> PayrollAdjustmentTemplates { get; }
    DbSet<PayrollAdjustment> PayrollAdjustments { get; }
    DbSet<PayrollSettings> PayrollSettings { get; }

    // ── Cash advances ────────────────────────────────────────────────────────
    DbSet<CashAdvance> CashAdvances { get; }

    // ── Notifications ───────────────────────────────────────────────────────
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }

    // ── Government contribution brackets ──────────────────────────────────
    DbSet<GovernmentContributionBracket> GovernmentContributionBrackets { get; }

    // ── Subscriptions & billing ─────────────────────────────────────────────
    DbSet<TenantSubscription> TenantSubscriptions { get; }
    DbSet<BillingRecord> BillingRecords { get; }
    DbSet<PlanChangeLog> PlanChangeLogs { get; }

    // ── Audit logs ──────────────────────────────────────────────────────────
    DbSet<AuditLog> AuditLogs { get; }

    // ── Expenses ────────────────────────────────────────────────────────────
    DbSet<ExpenseCategory> ExpenseCategories { get; }
    DbSet<Expense> Expenses { get; }

    // ── Loyalty ───────────────────────────────────────────────────────────
    DbSet<LoyaltyProgramSettings> LoyaltyProgramSettings { get; }
    DbSet<LoyaltyTierConfig> LoyaltyTierConfigs { get; }
    DbSet<LoyaltyReward> LoyaltyRewards { get; }
    DbSet<MembershipCard> MembershipCards { get; }
    DbSet<PointTransaction> PointTransactions { get; }

    // ── Inventory ────────────────────────────────────────────────────────────
    DbSet<SupplyCategory> SupplyCategories { get; }
    DbSet<SupplyItem> SupplyItems { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderLine> PurchaseOrderLines { get; }
    DbSet<Equipment> Equipment { get; }
    DbSet<MaintenanceLog> MaintenanceLogs { get; }
    DbSet<ServiceSupplyUsage> ServiceSupplyUsages { get; }

    // ── Franchise ────────────────────────────────────────────────────────────
    DbSet<FranchiseSettings> FranchiseSettings { get; }
    DbSet<FranchiseAgreement> FranchiseAgreements { get; }
    DbSet<RoyaltyPeriod> RoyaltyPeriods { get; }
    DbSet<FranchiseServiceTemplate> FranchiseServiceTemplates { get; }
    DbSet<FranchiseInvitation> FranchiseInvitations { get; }

    // ── Connect (customer-facing) ────────────────────────────────────────────
    DbSet<ConnectUser> ConnectUsers { get; }
    DbSet<ConnectUserTenantLink> ConnectUserTenantLinks { get; }
    DbSet<ConnectVehicle> ConnectVehicles { get; }
    DbSet<ConnectRefreshToken> ConnectRefreshTokens { get; }
    DbSet<GlobalMake> GlobalMakes { get; }
    DbSet<GlobalModel> GlobalModels { get; }
    DbSet<BookingSetting> BookingSettings { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<BookingService> BookingServices { get; }
    DbSet<Referral> Referrals { get; }
}
