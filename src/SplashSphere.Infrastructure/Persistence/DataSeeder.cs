using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Infrastructure.Auth;

namespace SplashSphere.Infrastructure.Persistence;

/// <summary>
/// Development-only seed data for SparkleWash Philippines.
/// Idempotent: a no-op if the seed tenant row already exists.
/// Call via <c>DataSeeder.SeedAsync(app.Services)</c> inside
/// <c>if (app.Environment.IsDevelopment())</c> in Program.cs.
/// </summary>
public static class DataSeeder
{
    // ── Tenant ────────────────────────────────────────────────────────────────
    private const string Ten = "org_sparklwash_dev";

    // ── Branches ──────────────────────────────────────────────────────────────
    private const string BrMkt = "branch-makati";
    private const string BrBgc = "branch-bgc";

    // ── Users (system logins — one cashier per branch) ────────────────────────
    private const string UsrMkt = "usr-cashier-mkt";
    private const string UsrBgc = "usr-cashier-bgc";

    // ── Vehicle Types ─────────────────────────────────────────────────────────
    private const string VtSedan = "vtype-sedan";
    private const string VtSuv   = "vtype-suv";
    private const string VtVan   = "vtype-van";
    private const string VtTruck = "vtype-truck";
    private const string VtMoto  = "vtype-moto";

    // ── Sizes ─────────────────────────────────────────────────────────────────
    private const string SzSm = "size-small";
    private const string SzMd = "size-medium";
    private const string SzLg = "size-large";
    private const string SzXl = "size-xl";

    // ── Makes ─────────────────────────────────────────────────────────────────
    private const string MkToyota    = "make-toyota";
    private const string MkHonda     = "make-honda";
    private const string MkMitsu     = "make-mitsubishi";
    private const string MkNissan    = "make-nissan";
    private const string MkSuzuki    = "make-suzuki";

    // ── Models ────────────────────────────────────────────────────────────────
    private const string MdVios      = "model-toyota-vios";
    private const string MdCorolla   = "model-toyota-corolla";
    private const string MdFortuner  = "model-toyota-fortuner";
    private const string MdHilux     = "model-toyota-hilux";
    private const string MdInnova    = "model-toyota-innova";
    private const string MdCity      = "model-honda-city";
    private const string MdCivic     = "model-honda-civic";
    private const string MdCrv       = "model-honda-crv";
    private const string MdJazz      = "model-honda-jazz";
    private const string MdMirage    = "model-mit-mirage";
    private const string MdMontero   = "model-mit-montero";
    private const string MdStrada    = "model-mit-strada";
    private const string MdAlmera    = "model-nissan-almera";
    private const string MdTerra     = "model-nissan-terra";
    private const string MdNavara    = "model-nissan-navara";
    private const string MdSwift     = "model-suzuki-swift";
    private const string MdErtiga    = "model-suzuki-ertiga";
    private const string MdJimny     = "model-suzuki-jimny";

    // ── Service Categories ────────────────────────────────────────────────────
    private const string CatExt  = "cat-exterior";
    private const string CatInt  = "cat-interior";
    private const string CatPrem = "cat-premium";

    // ── Merchandise Category ──────────────────────────────────────────────────
    private const string CatMerch = "cat-carcare";

    // ── Services ──────────────────────────────────────────────────────────────
    private const string SvcBasic  = "svc-basic-wash";
    private const string SvcPrem   = "svc-premium-wash";
    private const string SvcExpr   = "svc-express-wash";
    private const string SvcEng    = "svc-engine-bay";
    private const string SvcUnder  = "svc-undercarriage";
    private const string SvcVacuum = "svc-interior-vacuum";
    private const string SvcDash   = "svc-dashboard-wipe";
    private const string SvcFull   = "svc-full-interior";
    private const string SvcWax    = "svc-wax-polish";
    private const string SvcDetail = "svc-complete-detail";

    // ── Employees ─────────────────────────────────────────────────────────────
    private const string EMaria  = "emp-mkt-001"; // Commission
    private const string EJuan   = "emp-mkt-002"; // Commission
    private const string EPedro  = "emp-mkt-003"; // Commission
    private const string EAna    = "emp-mkt-004"; // Daily (cashier)
    private const string ECarlos = "emp-bgc-001"; // Commission
    private const string ERosa   = "emp-bgc-002"; // Commission
    private const string EMiguel = "emp-bgc-003"; // Commission
    private const string EElena  = "emp-bgc-004"; // Daily (cashier)

    // ── Merchandise ───────────────────────────────────────────────────────────
    private const string MAir   = "merch-airfresh";
    private const string MCloth = "merch-microfiber";
    private const string MShine = "merch-dashshine";
    private const string MTire  = "merch-tireblack";

    // ── Customers ─────────────────────────────────────────────────────────────
    private const string CJose    = "cust-jose-santos";
    private const string CMaria   = "cust-maria-cruz";
    private const string CRoberto = "cust-roberto-reyes";
    private const string CCarmela = "cust-carmela-mendoza";

    // ── Cars ──────────────────────────────────────────────────────────────────
    private const string CarVios    = "car-vios-abc1234";
    private const string CarCrv     = "car-crv-xyz5678";
    private const string CarStrada  = "car-strada-def9012";
    private const string CarFortu   = "car-fortuner-ghi3456";
    private const string CarCity    = "car-city-jkl7890";
    private const string CarTerra   = "car-terra-mno1234";
    private const string CarSwift   = "car-swift-pqr5678";
    private const string CarMontero = "car-montero-stu9012";

    // ── Transactions (≤ 26 chars — ULID slot) ────────────────────────────────
    private const string T01 = "SEEDMKT20260307001";
    private const string T02 = "SEEDMKT20260308001";
    private const string T03 = "SEEDMKT20260309001";
    private const string T04 = "SEEDMKT20260310001";
    private const string T05 = "SEEDMKT20260311001";
    private const string T06 = "SEEDMKT20260312001";
    private const string T07 = "SEEDBGC20260307001";
    private const string T08 = "SEEDBGC20260308001";
    private const string T09 = "SEEDBGC20260309001";
    private const string T10 = "SEEDBGC20260310001";
    private const string T11 = "SEEDBGC20260311001";
    private const string T12 = "SEEDBGC20260312001";

    // =========================================================================

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Global vehicle catalogue — runs independently of tenant seeding so the
        // Connect app always has makes/models available even in pre-seeded databases.
        await SeedGlobalVehicleCatalogueAsync(ctx);

        // Idempotency guard — Tenants has no global query filter so this always works
        if (await ctx.Tenants.AnyAsync(t => t.Id == Ten))
            return;

        // Set TenantContext so global filters evaluate correctly for any reads
        // performed by EF Core internals during this scope
        scope.ServiceProvider.GetRequiredService<TenantContext>().TenantId = Ten;

        // Batch 1 — root / master data (no FK dependencies on later batches)
        AddMasterData(ctx);
        await ctx.SaveChangesAsync();

        // Batch 2 — catalogue, matrices, employees, merchandise, cars, customers
        AddCatalogue(ctx);
        AddPricingMatrices(ctx);
        AddEmployeesAndMerchandise(ctx);
        AddCustomersAndCars(ctx);
        await ctx.SaveChangesAsync();

        // Batch 3 — transactions and all child records
        AddTransactions(ctx);
        await ctx.SaveChangesAsync();

        // Batch 4 — government contribution brackets (global, not tenant-scoped)
        SeedGovernmentBrackets(ctx);
        await ctx.SaveChangesAsync();

        // Batch 5 — subscription (dev tenant gets Growth/Active)
        SeedSubscription(ctx);
        await ctx.SaveChangesAsync();

        // Batch 6 — expense categories
        SeedExpenseCategories(ctx);
        await ctx.SaveChangesAsync();

        // Batch 7 — loyalty program
        SeedLoyaltyProgram(ctx);
        await ctx.SaveChangesAsync();

        // Batch 8 — supply categories
        SeedSupplyCategories(ctx);
        await ctx.SaveChangesAsync();
    }

    // ── Master data ───────────────────────────────────────────────────────────

    private static void AddMasterData(ApplicationDbContext ctx)
    {
        ctx.Add(new Tenant("org_sparklwash_dev",
            name: "SparkleWash Philippines",
            email: "admin@sparklewash.ph",
            contactNumber: "+63 917 123 4567",
            address: "Mandaluyong City, Metro Manila, Philippines") { Id = Ten });

        ctx.Add(new Branch(Ten, "Makati Branch", "MKT", "7849 Makati Ave, Makati City", "+63 2 8123 4567") { Id = BrMkt });
        ctx.Add(new Branch(Ten, "BGC Branch",    "BGC", "26th St, Bonifacio Global City, Taguig", "+63 2 8987 6543") { Id = BrBgc });

        // System users (POS cashier logins — one per branch)
        ctx.Add(new User("clerk_mkt_cashier_seed", "cashier-mkt@sparklewash.ph", "Ana",   "Lim")
            { Id = UsrMkt, TenantId = Ten });
        ctx.Add(new User("clerk_bgc_cashier_seed", "cashier-bgc@sparklewash.ph", "Elena", "Cruz")
            { Id = UsrBgc, TenantId = Ten });

        // Vehicle types
        ctx.Add(new VehicleType(Ten, "Sedan")      { Id = VtSedan });
        ctx.Add(new VehicleType(Ten, "SUV")        { Id = VtSuv   });
        ctx.Add(new VehicleType(Ten, "Van")        { Id = VtVan   });
        ctx.Add(new VehicleType(Ten, "Truck")      { Id = VtTruck });
        ctx.Add(new VehicleType(Ten, "Motorcycle") { Id = VtMoto  });

        // Sizes
        ctx.Add(new Size(Ten, "Small")  { Id = SzSm });
        ctx.Add(new Size(Ten, "Medium") { Id = SzMd });
        ctx.Add(new Size(Ten, "Large")  { Id = SzLg });
        ctx.Add(new Size(Ten, "XL")     { Id = SzXl });

        // Makes
        ctx.Add(new Make(Ten, "Toyota")    { Id = MkToyota });
        ctx.Add(new Make(Ten, "Honda")     { Id = MkHonda  });
        ctx.Add(new Make(Ten, "Mitsubishi") { Id = MkMitsu });
        ctx.Add(new Make(Ten, "Nissan")    { Id = MkNissan });
        ctx.Add(new Make(Ten, "Suzuki")    { Id = MkSuzuki });

        // Models — Toyota
        ctx.Add(new Model(Ten, MkToyota, "Vios")          { Id = MdVios     });
        ctx.Add(new Model(Ten, MkToyota, "Corolla Altis") { Id = MdCorolla  });
        ctx.Add(new Model(Ten, MkToyota, "Fortuner")      { Id = MdFortuner });
        ctx.Add(new Model(Ten, MkToyota, "Hilux")         { Id = MdHilux    });
        ctx.Add(new Model(Ten, MkToyota, "Innova")        { Id = MdInnova   });
        // Models — Honda
        ctx.Add(new Model(Ten, MkHonda, "City")  { Id = MdCity  });
        ctx.Add(new Model(Ten, MkHonda, "Civic") { Id = MdCivic });
        ctx.Add(new Model(Ten, MkHonda, "CR-V")  { Id = MdCrv   });
        ctx.Add(new Model(Ten, MkHonda, "Jazz")  { Id = MdJazz  });
        // Models — Mitsubishi
        ctx.Add(new Model(Ten, MkMitsu, "Mirage")        { Id = MdMirage  });
        ctx.Add(new Model(Ten, MkMitsu, "Montero Sport") { Id = MdMontero });
        ctx.Add(new Model(Ten, MkMitsu, "Strada")        { Id = MdStrada  });
        // Models — Nissan
        ctx.Add(new Model(Ten, MkNissan, "Almera") { Id = MdAlmera });
        ctx.Add(new Model(Ten, MkNissan, "Terra")  { Id = MdTerra  });
        ctx.Add(new Model(Ten, MkNissan, "Navara") { Id = MdNavara });
        // Models — Suzuki
        ctx.Add(new Model(Ten, MkSuzuki, "Swift")  { Id = MdSwift  });
        ctx.Add(new Model(Ten, MkSuzuki, "Ertiga") { Id = MdErtiga });
        ctx.Add(new Model(Ten, MkSuzuki, "Jimny")  { Id = MdJimny  });
    }

    // ── Service & merchandise catalogue ──────────────────────────────────────

    private static void AddCatalogue(ApplicationDbContext ctx)
    {
        ctx.Add(new ServiceCategory(Ten, "Exterior",  "Exterior cleaning and washing services") { Id = CatExt  });
        ctx.Add(new ServiceCategory(Ten, "Interior",  "Interior cleaning and detailing services") { Id = CatInt });
        ctx.Add(new ServiceCategory(Ten, "Premium",   "Premium detailing and full-service packages") { Id = CatPrem });
        ctx.Add(new MerchandiseCategory(Ten, "Car Care Products", "Retail car care and accessories") { Id = CatMerch });

        // 10 services across 3 categories
        ctx.Add(new Service(Ten, CatExt,  "Basic Exterior Wash",   150m,  "Standard exterior hand wash") { Id = SvcBasic  });
        ctx.Add(new Service(Ten, CatExt,  "Premium Exterior Wash", 280m,  "Premium exterior wash with rinse and dry") { Id = SvcPrem   });
        ctx.Add(new Service(Ten, CatExt,  "Express Wash",          120m,  "Quick exterior rinse — ideal for light dust") { Id = SvcExpr   });
        ctx.Add(new Service(Ten, CatExt,  "Engine Bay Cleaning",   450m,  "Thorough engine compartment degreasing") { Id = SvcEng    });
        ctx.Add(new Service(Ten, CatExt,  "Undercarriage Wash",    180m,  "High-pressure undercarriage rinse") { Id = SvcUnder  });
        ctx.Add(new Service(Ten, CatInt,  "Interior Vacuum",       150m,  "Full cabin vacuum including seats and mats") { Id = SvcVacuum });
        ctx.Add(new Service(Ten, CatInt,  "Dashboard Wipe",        100m,  "Dashboard, console and door panel wipe-down") { Id = SvcDash   });
        ctx.Add(new Service(Ten, CatInt,  "Full Interior Clean",   380m,  "Complete interior vacuum, wipe and shampoo") { Id = SvcFull   });
        ctx.Add(new Service(Ten, CatPrem, "Wax & Polish",          550m,  "Hand wax application and machine polish") { Id = SvcWax    });
        ctx.Add(new Service(Ten, CatPrem, "Complete Detail",      1200m,  "Full exterior + interior detail package") { Id = SvcDetail });
    }

    // ── Pricing & commission matrices ─────────────────────────────────────────

    private static void AddPricingMatrices(ApplicationDbContext ctx)
    {
        // ── Basic Exterior Wash — FULL 20-row pricing matrix ──────────────────
        ctx.AddRange(
            P(SvcBasic, VtSedan, SzSm,  150m), P(SvcBasic, VtSedan, SzMd, 180m),
            P(SvcBasic, VtSedan, SzLg,  200m), P(SvcBasic, VtSedan, SzXl, 220m),
            P(SvcBasic, VtSuv,   SzSm,  180m), P(SvcBasic, VtSuv,   SzMd, 220m),
            P(SvcBasic, VtSuv,   SzLg,  260m), P(SvcBasic, VtSuv,   SzXl, 300m),
            P(SvcBasic, VtVan,   SzSm,  200m), P(SvcBasic, VtVan,   SzMd, 230m),
            P(SvcBasic, VtVan,   SzLg,  270m), P(SvcBasic, VtVan,   SzXl, 310m),
            P(SvcBasic, VtTruck, SzSm,  200m), P(SvcBasic, VtTruck, SzMd, 240m),
            P(SvcBasic, VtTruck, SzLg,  280m), P(SvcBasic, VtTruck, SzXl, 320m),
            P(SvcBasic, VtMoto,  SzSm,  100m), P(SvcBasic, VtMoto,  SzMd, 120m),
            P(SvcBasic, VtMoto,  SzLg,  140m), P(SvcBasic, VtMoto,  SzXl, 160m));

        // ── Basic Exterior Wash — FULL 20-row commission matrix (15%) ─────────
        ctx.AddRange(
            Pct(SvcBasic, VtSedan, SzSm, 15m), Pct(SvcBasic, VtSedan, SzMd, 15m),
            Pct(SvcBasic, VtSedan, SzLg, 15m), Pct(SvcBasic, VtSedan, SzXl, 15m),
            Pct(SvcBasic, VtSuv,   SzSm, 15m), Pct(SvcBasic, VtSuv,   SzMd, 15m),
            Pct(SvcBasic, VtSuv,   SzLg, 15m), Pct(SvcBasic, VtSuv,   SzXl, 15m),
            Pct(SvcBasic, VtVan,   SzSm, 15m), Pct(SvcBasic, VtVan,   SzMd, 15m),
            Pct(SvcBasic, VtVan,   SzLg, 15m), Pct(SvcBasic, VtVan,   SzXl, 15m),
            Pct(SvcBasic, VtTruck, SzSm, 15m), Pct(SvcBasic, VtTruck, SzMd, 15m),
            Pct(SvcBasic, VtTruck, SzLg, 15m), Pct(SvcBasic, VtTruck, SzXl, 15m),
            Pct(SvcBasic, VtMoto,  SzSm, 15m), Pct(SvcBasic, VtMoto,  SzMd, 15m),
            Pct(SvcBasic, VtMoto,  SzLg, 15m), Pct(SvcBasic, VtMoto,  SzXl, 15m));

        // ── Express Wash — partial pricing matrix ─────────────────────────────
        ctx.AddRange(
            P(SvcExpr, VtSedan, SzSm,  120m), P(SvcExpr, VtSedan, SzMd, 140m),
            P(SvcExpr, VtSedan, SzLg,  160m), P(SvcExpr, VtSedan, SzXl, 180m),
            P(SvcExpr, VtSuv,   SzSm,  150m), P(SvcExpr, VtSuv,   SzMd, 180m),
            P(SvcExpr, VtSuv,   SzLg,  210m), P(SvcExpr, VtSuv,   SzXl, 240m),
            P(SvcExpr, VtTruck, SzLg,  220m),
            P(SvcExpr, VtMoto,  SzSm,   80m));

        // ── Express Wash — commission (10%) ───────────────────────────────────
        ctx.AddRange(
            Pct(SvcExpr, VtSedan, SzSm, 10m), Pct(SvcExpr, VtSedan, SzMd, 10m),
            Pct(SvcExpr, VtSedan, SzLg, 10m), Pct(SvcExpr, VtSedan, SzXl, 10m),
            Pct(SvcExpr, VtSuv,   SzSm, 10m), Pct(SvcExpr, VtSuv,   SzMd, 10m),
            Pct(SvcExpr, VtSuv,   SzLg, 10m), Pct(SvcExpr, VtSuv,   SzXl, 10m),
            Pct(SvcExpr, VtTruck, SzLg, 10m),
            Pct(SvcExpr, VtMoto,  SzSm, 10m));

        // ── Premium Exterior Wash — partial pricing matrix ────────────────────
        ctx.AddRange(
            P(SvcPrem, VtSedan, SzSm,  280m), P(SvcPrem, VtSedan, SzMd,  320m),
            P(SvcPrem, VtSuv,   SzSm,  330m), P(SvcPrem, VtSuv,   SzMd,  380m),
            P(SvcPrem, VtSuv,   SzLg,  430m), P(SvcPrem, VtSuv,   SzXl,  480m),
            P(SvcPrem, VtVan,   SzLg,  430m), P(SvcPrem, VtTruck, SzLg,  430m),
            P(SvcPrem, VtMoto,  SzSm,  200m));

        // ── Premium Exterior Wash — commission (15%) ──────────────────────────
        ctx.AddRange(
            Pct(SvcPrem, VtSedan, SzSm, 15m), Pct(SvcPrem, VtSedan, SzMd, 15m),
            Pct(SvcPrem, VtSuv,   SzSm, 15m), Pct(SvcPrem, VtSuv,   SzMd, 15m),
            Pct(SvcPrem, VtSuv,   SzLg, 15m), Pct(SvcPrem, VtSuv,   SzXl, 15m),
            Pct(SvcPrem, VtVan,   SzLg, 15m), Pct(SvcPrem, VtTruck, SzLg, 15m),
            Pct(SvcPrem, VtMoto,  SzSm, 15m));

        // ── Other services — targeted entries for seed transaction combos ─────
        // Interior Vacuum
        ctx.AddRange(P(SvcVacuum, VtSedan, SzSm, 150m), P(SvcVacuum, VtSuv, SzLg, 150m));
        ctx.AddRange(Pct(SvcVacuum, VtSedan, SzSm, 10m), Pct(SvcVacuum, VtSuv, SzLg, 10m));

        // Dashboard Wipe (pricing only — no commission; low-skill admin task)
        ctx.AddRange(P(SvcDash, VtSedan, SzSm, 100m), P(SvcDash, VtSuv, SzLg, 100m));

        // Undercarriage Wash
        ctx.AddRange(
            P(SvcUnder, VtTruck, SzLg, 210m), P(SvcUnder, VtSuv, SzLg, 200m),
            P(SvcUnder, VtSedan, SzSm, 150m));
        ctx.AddRange(
            Pct(SvcUnder, VtTruck, SzLg, 10m), Pct(SvcUnder, VtSuv, SzLg, 10m),
            Pct(SvcUnder, VtSedan, SzSm, 10m));

        // Full Interior Clean
        ctx.AddRange(P(SvcFull, VtSuv, SzLg, 400m), P(SvcFull, VtSedan, SzSm, 380m));
        ctx.AddRange(Pct(SvcFull, VtSuv, SzLg, 15m), Pct(SvcFull, VtSedan, SzSm, 15m));

        // Wax & Polish
        ctx.AddRange(P(SvcWax, VtSedan, SzSm, 600m), P(SvcWax, VtSuv, SzLg, 700m));
        ctx.AddRange(Pct(SvcWax, VtSedan, SzSm, 15m), Pct(SvcWax, VtSuv, SzLg, 15m));

        // Complete Detail
        ctx.AddRange(P(SvcDetail, VtSuv, SzLg, 1300m), P(SvcDetail, VtSedan, SzSm, 1200m));
        ctx.AddRange(Pct(SvcDetail, VtSuv, SzLg, 15m), Pct(SvcDetail, VtSedan, SzSm, 15m));

        // Engine Bay Cleaning
        ctx.AddRange(
            P(SvcEng, VtSedan, SzSm, 400m), P(SvcEng, VtSuv, SzLg, 500m),
            P(SvcEng, VtTruck, SzLg, 550m));
        ctx.AddRange(
            Pct(SvcEng, VtSedan, SzSm, 10m), Pct(SvcEng, VtSuv, SzLg, 10m),
            Pct(SvcEng, VtTruck, SzLg, 10m));
    }

    // ── Employees & merchandise ───────────────────────────────────────────────

    private static void AddEmployeesAndMerchandise(ApplicationDbContext ctx)
    {
        var hired24 = new DateOnly(2024, 1, 15);
        var hired24b = new DateOnly(2024, 6, 1);

        // Makati — 3 commission-type washers + 1 daily-rate cashier
        ctx.Add(new Employee(Ten, BrMkt, "Maria",  "Santos",   EmployeeType.Commission, hiredDate: hired24)  { Id = EMaria  });
        ctx.Add(new Employee(Ten, BrMkt, "Juan",   "dela Cruz", EmployeeType.Commission, hiredDate: hired24)  { Id = EJuan   });
        ctx.Add(new Employee(Ten, BrMkt, "Pedro",  "Reyes",    EmployeeType.Commission, hiredDate: new DateOnly(2024, 3, 1)) { Id = EPedro  });
        ctx.Add(new Employee(Ten, BrMkt, "Ana",    "Lim",      EmployeeType.Daily, dailyRate: 600m, hiredDate: hired24)  { Id = EAna    });

        // BGC — 3 commission-type washers + 1 daily-rate cashier
        ctx.Add(new Employee(Ten, BrBgc, "Carlos", "Mendoza",  EmployeeType.Commission, hiredDate: hired24b) { Id = ECarlos });
        ctx.Add(new Employee(Ten, BrBgc, "Rosa",   "Garcia",   EmployeeType.Commission, hiredDate: hired24b) { Id = ERosa   });
        ctx.Add(new Employee(Ten, BrBgc, "Miguel", "Torres",   EmployeeType.Commission, hiredDate: new DateOnly(2024, 8, 15)) { Id = EMiguel });
        ctx.Add(new Employee(Ten, BrBgc, "Elena",  "Cruz",     EmployeeType.Daily, dailyRate: 650m, hiredDate: hired24b) { Id = EElena  });

        // Merchandise category + 4 SKUs
        ctx.Add(new Merchandise(Ten, "Car Air Freshener",      "AF-001", 80m,  stockQuantity: 50,  lowStockThreshold: 10, categoryId: CatMerch, costPrice: 40m)  { Id = MAir   });
        ctx.Add(new Merchandise(Ten, "Microfiber Cloth",       "MC-001", 120m, stockQuantity: 100, lowStockThreshold: 20, categoryId: CatMerch, costPrice: 60m)  { Id = MCloth });
        ctx.Add(new Merchandise(Ten, "Dashboard Shine Spray",  "DS-001", 150m, stockQuantity: 30,  lowStockThreshold: 5,  categoryId: CatMerch, costPrice: 75m)  { Id = MShine });
        ctx.Add(new Merchandise(Ten, "Tire Black Spray",       "TB-001", 100m, stockQuantity: 40,  lowStockThreshold: 8,  categoryId: CatMerch, costPrice: 50m)  { Id = MTire  });
    }

    // ── Customers & cars ──────────────────────────────────────────────────────

    private static void AddCustomersAndCars(ApplicationDbContext ctx)
    {
        ctx.Add(new Customer(Ten, "Jose",    "Santos",  email: "jose.santos@email.com",  contactNumber: "+63 917 100 0001") { Id = CJose    });
        ctx.Add(new Customer(Ten, "Maria",   "Cruz",    email: "maria.cruz@email.com",   contactNumber: "+63 917 100 0002") { Id = CMaria   });
        ctx.Add(new Customer(Ten, "Roberto", "Reyes",   email: "roberto.reyes@email.com",contactNumber: "+63 917 100 0003") { Id = CRoberto });
        ctx.Add(new Customer(Ten, "Carmela", "Mendoza", email: "carmela.m@email.com",    contactNumber: "+63 917 100 0004") { Id = CCarmela });

        // Cars — registered vehicles (some walk-in, no customer FK)
        ctx.Add(new Car(Ten, VtSedan, SzSm, "ABC 1234", CJose,    MkToyota, MdVios,    "Red",    2020) { Id = CarVios    });
        ctx.Add(new Car(Ten, VtSuv,   SzLg, "XYZ 5678", CMaria,   MkHonda,  MdCrv,     "Silver", 2022) { Id = CarCrv     });
        ctx.Add(new Car(Ten, VtTruck, SzLg, "DEF 9012", null,      MkMitsu,  MdStrada,  "Black",  2021) { Id = CarStrada  });
        ctx.Add(new Car(Ten, VtSuv,   SzXl, "GHI 3456", CRoberto, MkToyota, MdFortuner,"White",  2023) { Id = CarFortu   });
        ctx.Add(new Car(Ten, VtSedan, SzSm, "JKL 7890", null,      MkHonda,  MdCity,    "Blue",   2019) { Id = CarCity    });
        ctx.Add(new Car(Ten, VtSuv,   SzLg, "MNO 1234", null,      MkNissan, MdTerra,   "Gray",   2022) { Id = CarTerra   });
        ctx.Add(new Car(Ten, VtSedan, SzSm, "PQR 5678", CCarmela, MkSuzuki, MdSwift,   "Red",    2021) { Id = CarSwift   });
        ctx.Add(new Car(Ten, VtSuv,   SzXl, "STU 9012", null,      MkMitsu,  MdMontero, "White",  2020) { Id = CarMontero });
    }

    // ── Sample transactions ───────────────────────────────────────────────────

    private static void AddTransactions(ApplicationDbContext ctx)
    {
        var now = DateTime.UtcNow;

        // ── TXN 01  MKT | Sedan Small | Jose's Vios | Completed ───────────────
        // Services: Basic Wash (150) + Interior Vacuum (150)
        // Employees: Maria, Juan (2-way split on each service)
        {
            var txn = Txn(T01, BrMkt, UsrMkt, CarVios, CJose,
                "MKT-20260307-0001", 300m, TransactionStatus.Completed, completedAt: now);
            var ts1 = Ts("ts-t01-1", T01, SvcBasic, VtSedan, SzSm, 150m, 22.50m);
            var ts2 = Ts("ts-t01-2", T01, SvcVacuum, VtSedan, SzSm, 150m, 15.00m);
            ctx.AddRange(txn, ts1, ts2);
            ctx.AddRange(
                Sea("sea-t01-1", ts1.Id, EMaria, 11.25m),
                Sea("sea-t01-2", ts1.Id, EJuan,  11.25m),
                Sea("sea-t01-3", ts2.Id, EMaria,  7.50m),
                Sea("sea-t01-4", ts2.Id, EJuan,   7.50m));
            ctx.AddRange(
                Te("te-t01-maria", T01, EMaria, 18.75m),
                Te("te-t01-juan",  T01, EJuan,  18.75m));
            ctx.Add(Pay("pay-t01", T01, PaymentMethod.Cash, 300m));
        }

        // ── TXN 02  MKT | SUV Large | Maria Cruz's CR-V | Completed ──────────
        // Services: Premium Wash (430) — 2 employees
        {
            var txn = Txn(T02, BrMkt, UsrMkt, CarCrv, CMaria,
                "MKT-20260308-0001", 430m, TransactionStatus.Completed, completedAt: now);
            var ts1 = Ts("ts-t02-1", T02, SvcPrem, VtSuv, SzLg, 430m, 64.50m);
            ctx.AddRange(txn, ts1);
            ctx.AddRange(
                Sea("sea-t02-1", ts1.Id, EPedro, 32.25m),
                Sea("sea-t02-2", ts1.Id, EMaria, 32.25m));
            ctx.AddRange(
                Te("te-t02-pedro", T02, EPedro, 32.25m),
                Te("te-t02-maria", T02, EMaria, 32.25m));
            ctx.Add(Pay("pay-t02", T02, PaymentMethod.GCash, 430m));
        }

        // ── TXN 03  MKT | Truck Large | Walk-in Strada | Completed ───────────
        // Services: Basic Wash (280) + Undercarriage (210) — 3 employees
        {
            var txn = Txn(T03, BrMkt, UsrMkt, CarStrada, null,
                "MKT-20260309-0001", 490m, TransactionStatus.Completed, completedAt: now);
            var ts1 = Ts("ts-t03-1", T03, SvcBasic, VtTruck, SzLg, 280m, 42.00m);
            var ts2 = Ts("ts-t03-2", T03, SvcUnder, VtTruck, SzLg, 210m, 21.00m);
            ctx.AddRange(txn, ts1, ts2);
            ctx.AddRange(
                Sea("sea-t03-1", ts1.Id, EMaria, 14.00m),
                Sea("sea-t03-2", ts1.Id, EJuan,  14.00m),
                Sea("sea-t03-3", ts1.Id, EPedro, 14.00m),
                Sea("sea-t03-4", ts2.Id, EMaria,  7.00m),
                Sea("sea-t03-5", ts2.Id, EJuan,   7.00m),
                Sea("sea-t03-6", ts2.Id, EPedro,  7.00m));
            ctx.AddRange(
                Te("te-t03-maria", T03, EMaria, 21.00m),
                Te("te-t03-juan",  T03, EJuan,  21.00m),
                Te("te-t03-pedro", T03, EPedro, 21.00m));
            ctx.Add(Pay("pay-t03", T03, PaymentMethod.Cash, 490m));
        }

        // ── TXN 04  MKT | SUV XL | Roberto's Fortuner | Completed ───────────
        // Services: Express Wash (240) + merchandise: Air Freshener (80) — 1 employee
        {
            var txn = Txn(T04, BrMkt, UsrMkt, CarFortu, CRoberto,
                "MKT-20260310-0001", 320m, TransactionStatus.Completed, completedAt: now);
            var ts1 = Ts("ts-t04-1", T04, SvcExpr, VtSuv, SzXl, 240m, 24.00m);
            ctx.AddRange(txn, ts1);
            ctx.Add(new TransactionMerchandise(Ten, T04, MAir, quantity: 1, unitPrice: 80m) { Id = "tm-t04-air" });
            ctx.Add(Sea("sea-t04-1", ts1.Id, EMaria, 24.00m));
            ctx.Add(Te("te-t04-maria", T04, EMaria, 24.00m));
            ctx.Add(Pay("pay-t04", T04, PaymentMethod.Cash, 320m));
        }

        // ── TXN 05  MKT | Sedan Small | Walk-in City | Completed ─────────────
        // Services: Basic Wash (150) — 2 employees
        {
            var txn = Txn(T05, BrMkt, UsrMkt, CarCity, null,
                "MKT-20260311-0001", 150m, TransactionStatus.Completed, completedAt: now);
            var ts1 = Ts("ts-t05-1", T05, SvcBasic, VtSedan, SzSm, 150m, 22.50m);
            ctx.AddRange(txn, ts1);
            ctx.AddRange(
                Sea("sea-t05-1", ts1.Id, EJuan,  11.25m),
                Sea("sea-t05-2", ts1.Id, EPedro, 11.25m));
            ctx.AddRange(
                Te("te-t05-juan",  T05, EJuan,  11.25m),
                Te("te-t05-pedro", T05, EPedro, 11.25m));
            ctx.Add(Pay("pay-t05", T05, PaymentMethod.Cash, 150m));
        }

        // ── TXN 06  MKT | SUV Large | Walk-in Terra | InProgress ─────────────
        // Services: Full Interior (400) + Undercarriage (200) — 2 employees, no payment yet
        {
            var txn = Txn(T06, BrMkt, UsrMkt, CarTerra, null,
                "MKT-20260312-0001", 600m, TransactionStatus.InProgress);
            var ts1 = Ts("ts-t06-1", T06, SvcFull,  VtSuv, SzLg, 400m, 60.00m);
            var ts2 = Ts("ts-t06-2", T06, SvcUnder, VtSuv, SzLg, 200m, 20.00m);
            ctx.AddRange(txn, ts1, ts2);
            ctx.AddRange(
                Sea("sea-t06-1", ts1.Id, EMaria, 30.00m),
                Sea("sea-t06-2", ts1.Id, EJuan,  30.00m),
                Sea("sea-t06-3", ts2.Id, EMaria, 10.00m),
                Sea("sea-t06-4", ts2.Id, EJuan,  10.00m));
            ctx.AddRange(
                Te("te-t06-maria", T06, EMaria, 40.00m),
                Te("te-t06-juan",  T06, EJuan,  40.00m));
        }

        // ── TXN 07  BGC | Sedan Small | Carmela's Swift | Completed ──────────
        // Services: Basic Wash (150) — 2 employees
        {
            var txn = Txn(T07, BrBgc, UsrBgc, CarSwift, CCarmela,
                "BGC-20260307-0001", 150m, TransactionStatus.Completed, completedAt: now);
            var ts1 = Ts("ts-t07-1", T07, SvcBasic, VtSedan, SzSm, 150m, 22.50m);
            ctx.AddRange(txn, ts1);
            ctx.AddRange(
                Sea("sea-t07-1", ts1.Id, ECarlos, 11.25m),
                Sea("sea-t07-2", ts1.Id, ERosa,   11.25m));
            ctx.AddRange(
                Te("te-t07-carlos", T07, ECarlos, 11.25m),
                Te("te-t07-rosa",   T07, ERosa,   11.25m));
            ctx.Add(Pay("pay-t07", T07, PaymentMethod.Cash, 150m));
        }

        // ── TXN 08  BGC | SUV XL | Walk-in Montero | Completed ───────────────
        // Services: Express Wash (240) — 1 employee
        {
            var txn = Txn(T08, BrBgc, UsrBgc, CarMontero, null,
                "BGC-20260308-0001", 240m, TransactionStatus.Completed, completedAt: now);
            var ts1 = Ts("ts-t08-1", T08, SvcExpr, VtSuv, SzXl, 240m, 24.00m);
            ctx.AddRange(txn, ts1);
            ctx.Add(Sea("sea-t08-1", ts1.Id, EMiguel, 24.00m));
            ctx.Add(Te("te-t08-miguel", T08, EMiguel, 24.00m));
            ctx.Add(Pay("pay-t08", T08, PaymentMethod.GCash, 240m));
        }

        // ── TXN 09  BGC | Sedan Small | Jose Santos | Completed ──────────────
        // Services: Basic Wash (150) + Wax & Polish (600) — 3 employees
        {
            var txn = Txn(T09, BrBgc, UsrBgc, CarVios, CJose,
                "BGC-20260309-0001", 750m, TransactionStatus.Completed, completedAt: now);
            var ts1 = Ts("ts-t09-1", T09, SvcBasic, VtSedan, SzSm, 150m,  22.50m);
            var ts2 = Ts("ts-t09-2", T09, SvcWax,   VtSedan, SzSm, 600m,  90.00m);
            ctx.AddRange(txn, ts1, ts2);
            ctx.AddRange(
                Sea("sea-t09-1", ts1.Id, ECarlos,  7.50m),
                Sea("sea-t09-2", ts1.Id, ERosa,    7.50m),
                Sea("sea-t09-3", ts1.Id, EMiguel,  7.50m),
                Sea("sea-t09-4", ts2.Id, ECarlos, 30.00m),
                Sea("sea-t09-5", ts2.Id, ERosa,   30.00m),
                Sea("sea-t09-6", ts2.Id, EMiguel, 30.00m));
            ctx.AddRange(
                Te("te-t09-carlos", T09, ECarlos, 37.50m),
                Te("te-t09-rosa",   T09, ERosa,   37.50m),
                Te("te-t09-miguel", T09, EMiguel, 37.50m));
            ctx.Add(Pay("pay-t09", T09, PaymentMethod.Cash, 750m));
        }

        // ── TXN 10  BGC | Sedan Small | Carmela's Swift | Completed ──────────
        // Services: Basic Wash (150) + Dashboard Wipe (100) + Microfiber Cloth (120) — 1 employee
        {
            var txn = Txn(T10, BrBgc, UsrBgc, CarSwift, CCarmela,
                "BGC-20260310-0001", 370m, TransactionStatus.Completed, completedAt: now);
            var ts1 = Ts("ts-t10-1", T10, SvcBasic, VtSedan, SzSm, 150m, 22.50m);
            var ts2 = Ts("ts-t10-2", T10, SvcDash,  VtSedan, SzSm, 100m,  0.00m); // no commission
            ctx.AddRange(txn, ts1, ts2);
            ctx.Add(new TransactionMerchandise(Ten, T10, MCloth, quantity: 1, unitPrice: 120m) { Id = "tm-t10-cloth" });
            ctx.Add(Sea("sea-t10-1", ts1.Id, ECarlos, 22.50m));
            ctx.Add(Te("te-t10-carlos", T10, ECarlos, 22.50m));
            ctx.Add(Pay("pay-t10", T10, PaymentMethod.Cash, 370m));
        }

        // ── TXN 11  BGC | Sedan Small | Walk-in | Cancelled ──────────────────
        // Services: Basic Wash (150) — 1 employee — no payment
        {
            var txn = Txn(T11, BrBgc, UsrBgc, CarCity, null,
                "BGC-20260311-0001", 150m, TransactionStatus.Cancelled, cancelledAt: now);
            var ts1 = Ts("ts-t11-1", T11, SvcBasic, VtSedan, SzSm, 150m, 22.50m);
            ctx.AddRange(txn, ts1);
            ctx.Add(Sea("sea-t11-1", ts1.Id, ERosa, 22.50m));
            ctx.Add(Te("te-t11-rosa", T11, ERosa, 22.50m));
        }

        // ── TXN 12  BGC | SUV Large | Maria Cruz's CR-V | Pending ────────────
        // Services: Complete Detail (1300) — 2 employees — awaiting payment
        {
            var txn = Txn(T12, BrBgc, UsrBgc, CarCrv, CMaria,
                "BGC-20260312-0001", 1300m, TransactionStatus.Pending);
            var ts1 = Ts("ts-t12-1", T12, SvcDetail, VtSuv, SzLg, 1300m, 195.00m);
            ctx.AddRange(txn, ts1);
            ctx.AddRange(
                Sea("sea-t12-1", ts1.Id, ERosa,   97.50m),
                Sea("sea-t12-2", ts1.Id, EMiguel, 97.50m));
            ctx.AddRange(
                Te("te-t12-rosa",   T12, ERosa,   97.50m),
                Te("te-t12-miguel", T12, EMiguel, 97.50m));
        }
    }

    // ── Factory helpers ───────────────────────────────────────────────────────

    private static Transaction Txn(
        string id, string branchId, string cashierId, string carId, string? customerId,
        string txnNumber, decimal total, TransactionStatus status,
        DateTime? completedAt = null, DateTime? cancelledAt = null) =>
        new(id, Ten, branchId, cashierId, carId, customerId)
        {
            TransactionNumber = txnNumber,
            TotalAmount       = total,
            DiscountAmount    = 0m,
            TaxAmount         = 0m,
            FinalAmount       = total,
            Status            = status,
            CompletedAt       = completedAt,
            CancelledAt       = cancelledAt,
        };

    // ── Government contribution brackets (2024-2025 PH rates) ─────────────
    private static void SeedGovernmentBrackets(ApplicationDbContext ctx)
    {
        // Skip if already seeded
        if (ctx.GovernmentContributionBrackets.Any()) return;

        var year = 2025;
        var brackets = new List<GovernmentContributionBracket>();
        var sort = 0;

        // SSS 2025 — simplified employee share brackets
        // Based on SSS Circular 2023-001 contribution table
        void AddSss(decimal min, decimal? max, decimal empShare)
        {
            brackets.Add(new GovernmentContributionBracket
            {
                DeductionType = "SSS", MinSalary = min, MaxSalary = max,
                EmployeeShare = empShare, Rate = 0, EffectiveYear = year, SortOrder = ++sort,
            });
        }

        AddSss(0m,       4_249.99m, 180m);
        AddSss(4_250m,   4_749.99m, 202.50m);
        AddSss(4_750m,   5_249.99m, 225m);
        AddSss(5_250m,   5_749.99m, 247.50m);
        AddSss(5_750m,   6_249.99m, 270m);
        AddSss(6_250m,   6_749.99m, 292.50m);
        AddSss(6_750m,   7_249.99m, 315m);
        AddSss(7_250m,   7_749.99m, 337.50m);
        AddSss(7_750m,   8_249.99m, 360m);
        AddSss(8_250m,   8_749.99m, 382.50m);
        AddSss(8_750m,   9_249.99m, 405m);
        AddSss(9_250m,   9_749.99m, 427.50m);
        AddSss(9_750m,   10_249.99m, 450m);
        AddSss(10_250m,  10_749.99m, 472.50m);
        AddSss(10_750m,  11_249.99m, 495m);
        AddSss(11_250m,  11_749.99m, 517.50m);
        AddSss(11_750m,  12_249.99m, 540m);
        AddSss(12_250m,  12_749.99m, 562.50m);
        AddSss(12_750m,  13_249.99m, 585m);
        AddSss(13_250m,  13_749.99m, 607.50m);
        AddSss(13_750m,  14_249.99m, 630m);
        AddSss(14_250m,  14_749.99m, 652.50m);
        AddSss(14_750m,  15_249.99m, 675m);
        AddSss(15_250m,  15_749.99m, 697.50m);
        AddSss(15_750m,  16_249.99m, 720m);
        AddSss(16_250m,  16_749.99m, 742.50m);
        AddSss(16_750m,  17_249.99m, 765m);
        AddSss(17_250m,  17_749.99m, 787.50m);
        AddSss(17_750m,  18_249.99m, 810m);
        AddSss(18_250m,  18_749.99m, 832.50m);
        AddSss(18_750m,  19_249.99m, 855m);
        AddSss(19_250m,  19_749.99m, 877.50m);
        AddSss(19_750m,  24_749.99m, 900m);
        AddSss(24_750m,  29_749.99m, 1_125m);
        AddSss(29_750m,  null, 1_350m);

        sort = 0;

        // PhilHealth 2025 — 5% of salary, split 50/50 (employee = 2.5%)
        // Rate-based: employee share = salary × 0.025, capped at monthly premium ceiling
        brackets.Add(new GovernmentContributionBracket
        {
            DeductionType = "PhilHealth", MinSalary = 0m, MaxSalary = 10_000m,
            EmployeeShare = 250m, Rate = 0, EffectiveYear = year, SortOrder = ++sort,
        });
        brackets.Add(new GovernmentContributionBracket
        {
            DeductionType = "PhilHealth", MinSalary = 10_000.01m, MaxSalary = 100_000m,
            EmployeeShare = 0m, Rate = 0.025m, EffectiveYear = year, SortOrder = ++sort,
        });
        brackets.Add(new GovernmentContributionBracket
        {
            DeductionType = "PhilHealth", MinSalary = 100_000.01m, MaxSalary = null,
            EmployeeShare = 2_500m, Rate = 0, EffectiveYear = year, SortOrder = ++sort,
        });

        sort = 0;

        // Pag-IBIG 2025 — employee contribution
        brackets.Add(new GovernmentContributionBracket
        {
            DeductionType = "PagIBIG", MinSalary = 0m, MaxSalary = 1_500m,
            EmployeeShare = 0m, Rate = 0.01m, EffectiveYear = year, SortOrder = ++sort,
        });
        brackets.Add(new GovernmentContributionBracket
        {
            DeductionType = "PagIBIG", MinSalary = 1_500.01m, MaxSalary = 5_000m,
            EmployeeShare = 0m, Rate = 0.02m, EffectiveYear = year, SortOrder = ++sort,
        });
        brackets.Add(new GovernmentContributionBracket
        {
            DeductionType = "PagIBIG", MinSalary = 5_000.01m, MaxSalary = null,
            EmployeeShare = 100m, Rate = 0, EffectiveYear = year, SortOrder = ++sort,
        });

        ctx.GovernmentContributionBrackets.AddRange(brackets);
    }

    private static TransactionService Ts(
        string id, string txnId, string svcId, string vtId, string szId,
        decimal unitPrice, decimal totalCommission) =>
        new(Ten, txnId, svcId, vtId, szId, unitPrice, totalCommission) { Id = id };

    private static ServiceEmployeeAssignment Sea(
        string id, string tsId, string empId, decimal commission) =>
        new(Ten, tsId, empId, commission) { Id = id };

    private static TransactionEmployee Te(
        string id, string txnId, string empId, decimal totalCommission) =>
        new(Ten, txnId, empId, totalCommission) { Id = id };

    private static Payment Pay(
        string id, string txnId, PaymentMethod method, decimal amount) =>
        new(Ten, txnId, method, amount) { Id = id };

    private static ServicePricing P(
        string svcId, string vtId, string szId, decimal price) =>
        new(Ten, svcId, vtId, szId, price);

    private static ServiceCommission Pct(
        string svcId, string vtId, string szId, decimal rate) =>
        new(Ten, svcId, vtId, szId, CommissionType.Percentage, null, rate);

    // ── Subscription seed ───────────────────────────────────────────────────

    private static void SeedSubscription(ApplicationDbContext ctx)
    {
        if (ctx.TenantSubscriptions.IgnoreQueryFilters().Any()) return;

        var now = DateTime.UtcNow;
        var sub = new TenantSubscription(Ten, PlanTier.Growth, SubscriptionStatus.Active)
        {
            TrialStartDate = now.AddDays(-30),
            TrialEndDate = now.AddDays(-16),
            CurrentPeriodStart = now.AddDays(-15),
            CurrentPeriodEnd = now.AddDays(15),
            LastPaymentDate = now.AddDays(-15),
            NextBillingDate = now.AddDays(15),
            SmsCountResetDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
        };

        ctx.TenantSubscriptions.Add(sub);
    }

    // ── Expense categories seed ─────────────────────────────────────────────

    private static void SeedExpenseCategories(ApplicationDbContext ctx)
    {
        if (ctx.ExpenseCategories.IgnoreQueryFilters().Any()) return;

        var categories = new[]
        {
            "Water Bill", "Electricity", "Rent", "Soap & Chemicals",
            "Equipment Maintenance", "Employee Meals/Snacks", "Transportation",
            "Supplies (towels, sponges)", "Miscellaneous", "Insurance", "Taxes & Permits"
        };

        foreach (var name in categories)
            ctx.ExpenseCategories.Add(new ExpenseCategory(Ten, name));
    }

    // ── Loyalty program seed ──────────────────────────────────────────────────

    private static void SeedLoyaltyProgram(ApplicationDbContext ctx)
    {
        if (ctx.LoyaltyProgramSettings.IgnoreQueryFilters().Any()) return;

        // Settings — 1 point per ₱100 spent, active, auto-enroll, 12-month expiry
        var settingsId = "loyalty-settings-1";
        var settings = new LoyaltyProgramSettings(Ten)
        {
            Id = settingsId,
            PointsPerCurrencyUnit = 1m,
            CurrencyUnitAmount = 100m,
            IsActive = true,
            AutoEnroll = true,
            PointsExpirationMonths = 12,
        };
        ctx.LoyaltyProgramSettings.Add(settings);

        // Tier configs
        ctx.LoyaltyTierConfigs.Add(new LoyaltyTierConfig(Ten, settingsId, LoyaltyTier.Standard, "Standard", 0, 1.0m));
        ctx.LoyaltyTierConfigs.Add(new LoyaltyTierConfig(Ten, settingsId, LoyaltyTier.Silver, "Silver", 500, 1.25m));
        ctx.LoyaltyTierConfigs.Add(new LoyaltyTierConfig(Ten, settingsId, LoyaltyTier.Gold, "Gold", 2000, 1.5m));
        ctx.LoyaltyTierConfigs.Add(new LoyaltyTierConfig(Ten, settingsId, LoyaltyTier.Platinum, "Platinum", 5000, 2.0m));

        // Rewards catalogue
        ctx.LoyaltyRewards.Add(new LoyaltyReward(Ten, "10% Off Next Wash", RewardType.DiscountPercent, 200) { Description = "10% discount on any single service", DiscountPercent = 10m });
        ctx.LoyaltyRewards.Add(new LoyaltyReward(Ten, "₱50 Off", RewardType.DiscountAmount, 100) { Description = "₱50 discount on any order", DiscountAmount = 50m });
        ctx.LoyaltyRewards.Add(new LoyaltyReward(Ten, "₱200 Off Premium", RewardType.DiscountAmount, 500) { Description = "₱200 off any premium service", DiscountAmount = 200m });

        // Membership cards for 2 existing customers
        var cardJose = new MembershipCard(Ten, CJose, "SS-00001")
        {
            CurrentTier = LoyaltyTier.Silver,
            PointsBalance = 650,
            LifetimePointsEarned = 850,
            LifetimePointsRedeemed = 200,
        };
        ctx.MembershipCards.Add(cardJose);

        var cardMaria = new MembershipCard(Ten, CMaria, "SS-00002")
        {
            CurrentTier = LoyaltyTier.Standard,
            PointsBalance = 120,
            LifetimePointsEarned = 120,
        };
        ctx.MembershipCards.Add(cardMaria);

        // Sample point transactions for Jose
        ctx.PointTransactions.Add(new PointTransaction(Ten, cardJose.Id, PointTransactionType.Earned, 350, 350, "Earned from MKT-20260315-001")
        {
            ExpiresAt = DateTime.UtcNow.AddMonths(12),
        });
        ctx.PointTransactions.Add(new PointTransaction(Ten, cardJose.Id, PointTransactionType.Earned, 500, 850, "Earned from MKT-20260320-003")
        {
            ExpiresAt = DateTime.UtcNow.AddMonths(12),
        });
        ctx.PointTransactions.Add(new PointTransaction(Ten, cardJose.Id, PointTransactionType.Redeemed, -200, 650, "Redeemed: ₱50 Off"));

        // Sample point transaction for Maria
        ctx.PointTransactions.Add(new PointTransaction(Ten, cardMaria.Id, PointTransactionType.Earned, 120, 120, "Earned from BGC-20260325-002")
        {
            ExpiresAt = DateTime.UtcNow.AddMonths(12),
        });
    }

    // ── Supply categories ─────────────────────────────────────────────────────

    private static void SeedSupplyCategories(ApplicationDbContext ctx)
    {
        ctx.SupplyCategories.Add(new SupplyCategory(Ten, "Cleaning Chemicals", "Car wash soaps, detergents, and degreasers") { Id = "supcat-cleaning" });
        ctx.SupplyCategories.Add(new SupplyCategory(Ten, "Wax & Polish", "Spray wax, polish compounds, and sealants") { Id = "supcat-wax" });
        ctx.SupplyCategories.Add(new SupplyCategory(Ten, "Tire & Trim Products", "Tire black, trim restorers, and protectants") { Id = "supcat-tire" });
        ctx.SupplyCategories.Add(new SupplyCategory(Ten, "Towels & Cloths", "Microfiber towels, chamois, and drying cloths") { Id = "supcat-towels" });
        ctx.SupplyCategories.Add(new SupplyCategory(Ten, "Brushes & Tools", "Wash mitts, brushes, and applicator pads") { Id = "supcat-brushes" });
        ctx.SupplyCategories.Add(new SupplyCategory(Ten, "Water & Utilities", "Water consumption and utility tracking") { Id = "supcat-water" });
        ctx.SupplyCategories.Add(new SupplyCategory(Ten, "Packaging & Miscellaneous", "Paper towels, plastic wraps, and misc supplies") { Id = "supcat-misc" });
    }

    // ── Global vehicle catalogue (Connect app) ────────────────────────────────
    //
    // Not tenant-scoped — seeded once per database. Covers the fifteen most
    // common Philippine passenger/light-truck brands. Adding a new make or model
    // later is safe: this method only inserts missing makes and missing models
    // (matched by name), so re-running the seeder will top up the catalogue
    // without duplicating existing rows.

    private static async Task SeedGlobalVehicleCatalogueAsync(ApplicationDbContext ctx)
    {
        var existingMakes = await ctx.GlobalMakes
            .Select(m => new { m.Id, m.Name })
            .ToListAsync();
        var existingMakeByName = existingMakes.ToDictionary(m => m.Name, m => m.Id, StringComparer.OrdinalIgnoreCase);

        var catalogue = new (string Make, string[] Models)[]
        {
            ("Toyota",     new[] { "Vios", "Wigo", "Corolla Altis", "Fortuner", "Innova", "Hilux", "Rush", "Avanza", "Raize", "Camry", "Land Cruiser" }),
            ("Mitsubishi", new[] { "Mirage", "Mirage G4", "Xpander", "Montero Sport", "Strada", "L300", "Xpander Cross" }),
            ("Honda",      new[] { "City", "Civic", "Jazz", "Brio", "BR-V", "CR-V", "HR-V", "Accord" }),
            ("Ford",       new[] { "Ranger", "Everest", "Territory", "EcoSport", "Mustang" }),
            ("Suzuki",     new[] { "Ertiga", "Swift", "Jimny", "Dzire", "Celerio", "Vitara", "XL7" }),
            ("Nissan",     new[] { "Almera", "Navara", "Terra", "Juke", "X-Trail", "Urvan", "Livina" }),
            ("Hyundai",    new[] { "Accent", "Elantra", "Tucson", "Santa Fe", "Reina", "Creta", "Stargazer" }),
            ("Kia",        new[] { "Picanto", "Rio", "Soluto", "Sportage", "Sorento", "Seltos", "Carnival", "Stonic" }),
            ("Isuzu",      new[] { "D-Max", "mu-X", "Traviz", "Crosswind" }),
            ("Chevrolet",  new[] { "Spark", "Trailblazer", "Colorado", "Captiva" }),
            ("Mazda",      new[] { "Mazda2", "Mazda3", "CX-3", "CX-5", "CX-8", "CX-9", "BT-50" }),
            ("MG",         new[] { "MG 5", "MG ZS", "MG RX5", "MG HS", "MG 3" }),
            ("Geely",      new[] { "Coolray", "Azkarra", "Emgrand", "Okavango", "Tugella" }),
            ("Chery",      new[] { "Tiggo 2", "Tiggo 5X", "Tiggo 7 Pro", "Tiggo 8 Pro" }),
            ("Subaru",     new[] { "Forester", "Outback", "XV", "WRX", "BRZ" }),
        };

        var order = 0;
        foreach (var (makeName, models) in catalogue)
        {
            if (!existingMakeByName.TryGetValue(makeName, out var makeId))
            {
                var newMake = new GlobalMake(makeName, order);
                makeId = newMake.Id;
                ctx.GlobalMakes.Add(newMake);
                existingMakeByName[makeName] = makeId;
            }
            order++;
        }

        // Flush makes first so models can reference them by id
        await ctx.SaveChangesAsync();

        var existingModels = await ctx.GlobalModels
            .Select(m => new { m.GlobalMakeId, m.Name })
            .ToListAsync();
        var existingModelKey = new HashSet<string>(
            existingModels.Select(m => $"{m.GlobalMakeId}|{m.Name.ToLowerInvariant()}"));

        foreach (var (makeName, models) in catalogue)
        {
            var makeId = existingMakeByName[makeName];
            var modelOrder = 0;
            foreach (var modelName in models)
            {
                var key = $"{makeId}|{modelName.ToLowerInvariant()}";
                if (existingModelKey.Add(key))
                {
                    ctx.GlobalModels.Add(new GlobalModel(makeId, modelName, modelOrder));
                }
                modelOrder++;
            }
        }

        await ctx.SaveChangesAsync();
    }
}
