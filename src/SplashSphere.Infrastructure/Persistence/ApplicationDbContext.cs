using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Interfaces;
using SplashSphere.Infrastructure.Auth;

namespace SplashSphere.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    TenantContext tenantContext)
    : DbContext(options), IApplicationDbContext
{
    // ── Tenant & identity ─────────────────────────────────────────────────────
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PosStation> PosStations => Set<PosStation>();

    // ── Vehicle master data ───────────────────────────────────────────────────
    public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
    public DbSet<Size> Sizes => Set<Size>();
    public DbSet<Make> Makes => Set<Make>();
    public DbSet<Model> Models => Set<Model>();

    // ── Service catalogue ─────────────────────────────────────────────────────
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServicePricing> ServicePricings => Set<ServicePricing>();
    public DbSet<ServiceCommission> ServiceCommissions => Set<ServiceCommission>();

    // ── Package catalogue ─────────────────────────────────────────────────────
    public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
    public DbSet<PackageService> PackageServices => Set<PackageService>();
    public DbSet<PackagePricing> PackagePricings => Set<PackagePricing>();
    public DbSet<PackageCommission> PackageCommissions => Set<PackageCommission>();

    // ── Pricing modifiers ─────────────────────────────────────────────────────
    public DbSet<PricingModifier> PricingModifiers => Set<PricingModifier>();

    // ── Customers & vehicles ──────────────────────────────────────────────────
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Car> Cars => Set<Car>();

    // ── Merchandise ───────────────────────────────────────────────────────────
    public DbSet<MerchandiseCategory> MerchandiseCategories => Set<MerchandiseCategory>();
    public DbSet<Merchandise> Merchandise => Set<Merchandise>();

    // ── Employees & payroll ───────────────────────────────────────────────────
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollEntry> PayrollEntries => Set<PayrollEntry>();

    // ── Transactions ──────────────────────────────────────────────────────────
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionService> TransactionServices => Set<TransactionService>();
    public DbSet<TransactionPackage> TransactionPackages => Set<TransactionPackage>();
    public DbSet<TransactionMerchandise> TransactionMerchandise => Set<TransactionMerchandise>();
    public DbSet<TransactionEmployee> TransactionEmployees => Set<TransactionEmployee>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ServiceEmployeeAssignment> ServiceEmployeeAssignments => Set<ServiceEmployeeAssignment>();
    public DbSet<PackageEmployeeAssignment> PackageEmployeeAssignments => Set<PackageEmployeeAssignment>();

    // ── Queue ─────────────────────────────────────────────────────────────────
    public DbSet<QueueEntry> QueueEntries => Set<QueueEntry>();

    // ── Cashier shifts ────────────────────────────────────────────────────────
    public DbSet<CashierShift> CashierShifts => Set<CashierShift>();
    public DbSet<CashMovement> CashMovements => Set<CashMovement>();
    public DbSet<ShiftDenomination> ShiftDenominations => Set<ShiftDenomination>();
    public DbSet<ShiftPaymentSummary> ShiftPaymentSummaries => Set<ShiftPaymentSummary>();
    public DbSet<ShiftSettings> ShiftSettings => Set<ShiftSettings>();

    // ── Payroll adjustment templates, adjustments & settings ────────────────
    public DbSet<PayrollAdjustmentTemplate> PayrollAdjustmentTemplates => Set<PayrollAdjustmentTemplate>();
    public DbSet<PayrollAdjustment> PayrollAdjustments => Set<PayrollAdjustment>();
    public DbSet<PayrollSettings> PayrollSettings => Set<PayrollSettings>();

    // ── Cash advances ────────────────────────────────────────────────────────
    public DbSet<CashAdvance> CashAdvances => Set<CashAdvance>();

    // ── Notifications ───────────────────────────────────────────────────────
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    // ── Government contribution brackets ──────────────────────────────────
    public DbSet<GovernmentContributionBracket> GovernmentContributionBrackets => Set<GovernmentContributionBracket>();

    // ── Subscriptions & billing ─────────────────────────────────────────────
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<BillingRecord> BillingRecords => Set<BillingRecord>();
    public DbSet<PlanChangeLog> PlanChangeLogs => Set<PlanChangeLog>();

    // ── Audit logs ──────────────────────────────────────────────────────────
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ── Expenses ────────────────────────────────────────────────────────────
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();

    // ── Receipt settings ────────────────────────────────────────────────────
    public DbSet<ReceiptSetting> ReceiptSettings => Set<ReceiptSetting>();

    // ── Display settings ────────────────────────────────────────────────────
    public DbSet<DisplaySetting> DisplaySettings => Set<DisplaySetting>();

    // ── Loyalty ───────────────────────────────────────────────────────────
    public DbSet<LoyaltyProgramSettings> LoyaltyProgramSettings => Set<LoyaltyProgramSettings>();
    public DbSet<LoyaltyTierConfig> LoyaltyTierConfigs => Set<LoyaltyTierConfig>();
    public DbSet<LoyaltyReward> LoyaltyRewards => Set<LoyaltyReward>();
    public DbSet<MembershipCard> MembershipCards => Set<MembershipCard>();
    public DbSet<PointTransaction> PointTransactions => Set<PointTransaction>();

    // ── Inventory ────────────────────────────────────────────────────────────
    public DbSet<SupplyCategory> SupplyCategories => Set<SupplyCategory>();
    public DbSet<SupplyItem> SupplyItems => Set<SupplyItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<MaintenanceLog> MaintenanceLogs => Set<MaintenanceLog>();
    public DbSet<ServiceSupplyUsage> ServiceSupplyUsages => Set<ServiceSupplyUsage>();

    // ── Franchise ────────────────────────────────────────────────────────────
    public DbSet<FranchiseSettings> FranchiseSettings => Set<FranchiseSettings>();
    public DbSet<FranchiseAgreement> FranchiseAgreements => Set<FranchiseAgreement>();
    public DbSet<RoyaltyPeriod> RoyaltyPeriods => Set<RoyaltyPeriod>();
    public DbSet<FranchiseServiceTemplate> FranchiseServiceTemplates => Set<FranchiseServiceTemplate>();
    public DbSet<FranchiseInvitation> FranchiseInvitations => Set<FranchiseInvitation>();

    // ── Connect (customer-facing) ────────────────────────────────────────────
    public DbSet<ConnectUser> ConnectUsers => Set<ConnectUser>();
    public DbSet<ConnectUserTenantLink> ConnectUserTenantLinks => Set<ConnectUserTenantLink>();
    public DbSet<ConnectVehicle> ConnectVehicles => Set<ConnectVehicle>();
    public DbSet<ConnectRefreshToken> ConnectRefreshTokens => Set<ConnectRefreshToken>();
    public DbSet<GlobalMake> GlobalMakes => Set<GlobalMake>();
    public DbSet<GlobalModel> GlobalModels => Set<GlobalModel>();
    public DbSet<BookingSetting> BookingSettings => Set<BookingSetting>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingService> BookingServices => Set<BookingService>();
    public DbSet<Referral> Referrals => Set<Referral>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // ── Tenant isolation: auto-register the standard filter ────────────────
        // Any entity tagged with the ITenantScoped marker interface gets a global
        // query filter `e => e.TenantId == tenantContext.TenantId` registered
        // automatically. Adding a new tenant-scoped entity requires only the
        // marker — there is no separate spot to remember to update here.
        // Use .IgnoreQueryFilters() in handlers that need cross-tenant access
        // (e.g. user lookup in TenantResolutionMiddleware, admin tooling).

        var applyMethod = typeof(ApplicationDbContext).GetMethod(
            nameof(ApplyTenantFilter),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                applyMethod
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, [modelBuilder]);
            }
        }

        // ── Non-standard tenant filters (kept hand-wired) ──────────────────────
        // Expense uses an additional !IsDeleted soft-delete predicate.
        modelBuilder.Entity<Expense>()
            .HasQueryFilter(e => e.TenantId == tenantContext.TenantId && !e.IsDeleted);

        // FranchiseServiceTemplate scopes by FranchisorTenantId (not TenantId)
        // because the catalogue is owned by the franchisor.
        modelBuilder.Entity<FranchiseServiceTemplate>()
            .HasQueryFilter(fst => fst.FranchisorTenantId == tenantContext.TenantId);

        // ── Intentionally unfiltered entities ──────────────────────────────────
        // Tenant: root record. ConnectUser, ConnectVehicle, ConnectRefreshToken,
        // GlobalMake, GlobalModel: global (not tenant-scoped). FranchiseAgreement,
        // RoyaltyPeriod, FranchiseInvitation: cross-tenant or public access.

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Registers the standard tenant query filter for one ITenantScoped entity.
    /// Called dynamically (via <see cref="MethodInfo.MakeGenericMethod"/>) for
    /// every CLR type implementing the marker. The lambda body uses ordinary
    /// C# closure capture over <c>tenantContext</c> — exactly the same pattern
    /// as the hand-wired filters above — so EF Core parameterizes the
    /// right-hand side at query time and re-evaluates it per request, instead
    /// of inlining the first request's value as a constant.
    /// </summary>
    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => EF.Property<string>(e, "TenantId") == tenantContext.TenantId);
    }
}
