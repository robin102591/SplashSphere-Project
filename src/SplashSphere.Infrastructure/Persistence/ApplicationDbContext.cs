using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
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

        // ── Global query filters — tenant isolation ────────────────────────────
        // Every query is automatically scoped to the current tenant.
        // Use .IgnoreQueryFilters() in handlers that need cross-tenant access
        // (e.g. user lookup in TenantResolutionMiddleware, admin tooling).

        // Tenant itself is never filtered — it IS the root tenant record
        // User: nullable TenantId (null during onboarding); filter when TenantId is set
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => u.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Branch>()
            .HasQueryFilter(b => b.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<VehicleType>()
            .HasQueryFilter(vt => vt.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Size>()
            .HasQueryFilter(s => s.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Make>()
            .HasQueryFilter(m => m.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Model>()
            .HasQueryFilter(m => m.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<ServiceCategory>()
            .HasQueryFilter(sc => sc.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Service>()
            .HasQueryFilter(s => s.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<ServicePricing>()
            .HasQueryFilter(sp => sp.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<ServiceCommission>()
            .HasQueryFilter(sc => sc.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<ServicePackage>()
            .HasQueryFilter(sp => sp.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<PackageService>()
            .HasQueryFilter(ps => ps.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<PackagePricing>()
            .HasQueryFilter(pp => pp.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<PackageCommission>()
            .HasQueryFilter(pc => pc.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<PricingModifier>()
            .HasQueryFilter(pm => pm.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<MerchandiseCategory>()
            .HasQueryFilter(mc => mc.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Merchandise>()
            .HasQueryFilter(m => m.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Customer>()
            .HasQueryFilter(c => c.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Car>()
            .HasQueryFilter(c => c.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Employee>()
            .HasQueryFilter(e => e.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Attendance>()
            .HasQueryFilter(a => a.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<PayrollPeriod>()
            .HasQueryFilter(pp => pp.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<PayrollEntry>()
            .HasQueryFilter(pe => pe.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Transaction>()
            .HasQueryFilter(t => t.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<TransactionService>()
            .HasQueryFilter(ts => ts.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<TransactionPackage>()
            .HasQueryFilter(tp => tp.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<TransactionMerchandise>()
            .HasQueryFilter(tm => tm.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<TransactionEmployee>()
            .HasQueryFilter(te => te.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Payment>()
            .HasQueryFilter(p => p.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<ServiceEmployeeAssignment>()
            .HasQueryFilter(sea => sea.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<PackageEmployeeAssignment>()
            .HasQueryFilter(pea => pea.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<QueueEntry>()
            .HasQueryFilter(qe => qe.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<CashierShift>()
            .HasQueryFilter(cs => cs.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<CashMovement>()
            .HasQueryFilter(cm => cm.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<ShiftDenomination>()
            .HasQueryFilter(sd => sd.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<ShiftPaymentSummary>()
            .HasQueryFilter(sp => sp.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<ShiftSettings>()
            .HasQueryFilter(ss => ss.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<PayrollAdjustmentTemplate>()
            .HasQueryFilter(pat => pat.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<PayrollAdjustment>()
            .HasQueryFilter(pa => pa.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<PayrollSettings>()
            .HasQueryFilter(ps => ps.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<CashAdvance>()
            .HasQueryFilter(ca => ca.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Notification>()
            .HasQueryFilter(n => n.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<NotificationPreference>()
            .HasQueryFilter(np => np.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<AuditLog>()
            .HasQueryFilter(a => a.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<ExpenseCategory>()
            .HasQueryFilter(ec => ec.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Expense>()
            .HasQueryFilter(e => e.TenantId == tenantContext.TenantId && !e.IsDeleted);

        modelBuilder.Entity<LoyaltyProgramSettings>()
            .HasQueryFilter(s => s.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<LoyaltyTierConfig>()
            .HasQueryFilter(t => t.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<LoyaltyReward>()
            .HasQueryFilter(r => r.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<MembershipCard>()
            .HasQueryFilter(m => m.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<PointTransaction>()
            .HasQueryFilter(p => p.TenantId == tenantContext.TenantId);

        // ── Inventory ────────────────────────────────────────────────────────
        modelBuilder.Entity<SupplyCategory>()
            .HasQueryFilter(sc => sc.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<SupplyItem>()
            .HasQueryFilter(si => si.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<StockMovement>()
            .HasQueryFilter(sm => sm.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Supplier>()
            .HasQueryFilter(s => s.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<PurchaseOrder>()
            .HasQueryFilter(po => po.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Equipment>()
            .HasQueryFilter(eq => eq.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<ServiceSupplyUsage>()
            .HasQueryFilter(ssu => ssu.TenantId == tenantContext.TenantId);

        // ── Franchise (tenant-scoped only) ───────────────────────────────────
        // NOTE: FranchiseAgreement, RoyaltyPeriod, and FranchiseInvitation are
        // explicitly NOT filtered — they bridge tenants or need public access.
        modelBuilder.Entity<FranchiseSettings>()
            .HasQueryFilter(fs => fs.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<FranchiseServiceTemplate>()
            .HasQueryFilter(fst => fst.FranchisorTenantId == tenantContext.TenantId);

        // ── Connect (customer-facing) ────────────────────────────────────────
        // NOTE: ConnectUser, ConnectVehicle, ConnectRefreshToken, GlobalMake, and
        // GlobalModel are intentionally NOT filtered — they are global (not tenant-scoped).
        modelBuilder.Entity<ConnectUserTenantLink>()
            .HasQueryFilter(l => l.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<BookingSetting>()
            .HasQueryFilter(s => s.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Booking>()
            .HasQueryFilter(b => b.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<BookingService>()
            .HasQueryFilter(bs => bs.TenantId == tenantContext.TenantId);

        modelBuilder.Entity<Referral>()
            .HasQueryFilter(r => r.TenantId == tenantContext.TenantId);

        base.OnModelCreating(modelBuilder);
    }
}
