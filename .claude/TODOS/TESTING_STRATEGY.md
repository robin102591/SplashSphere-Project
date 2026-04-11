# SplashSphere — Testing Strategy

> **Priority:** Build tests alongside features, not after. Focus on business-critical calculations first.
> **Frameworks:** xUnit + FluentAssertions + NSubstitute (backend), Vitest + Testing Library (frontend)
> **Target:** 80% coverage on domain/application layers, 60% on infrastructure, critical paths at 100%.

---

## Test Priority Order

Build tests in this order — highest business risk first:

### Tier 1: Critical (Test Immediately — Bugs Here Lose Money)

| What | Why | Type |
|---|---|---|
| Commission calculation (all 3 types) | Wrong commission = employee disputes = attrition | Unit |
| Commission split among N employees | Rounding errors compound across hundreds of transactions | Unit |
| Transaction total calculation | finalAmount = total - discount + tax must be exact | Unit |
| Payroll processing | COMMISSION + DAILY employee calculations, deductions | Unit |
| Cash advance deduction | Must not make net pay negative | Unit |
| Shift cash variance formula | ExpectedCash = Opening + CashPayments + CashIn - CashOut | Unit |
| Pricing matrix lookup | Correct price for service × vehicleType × size | Unit |
| Tenant isolation | Tenant A must never see Tenant B's data | Integration |
| Plan enforcement | Starter blocked from Growth features, limits respected | Integration |
| Offline transaction sync | Idempotent via OfflineTempId, no duplicates | Integration |

### Tier 2: Important (Test Before Launch)

| What | Why | Type |
|---|---|---|
| Service supply usage auto-deduction | Wrong deduction = inaccurate inventory | Unit |
| Cost-per-wash calculation | Feeds into AI advisor and P&L reports | Unit |
| Loyalty points earning/redemption | Double-spend prevention | Unit |
| Purchase order receive flow | Stock must update correctly on receive | Integration |
| Onboarding flow | Clerk org creation + seed data must complete atomically | Integration |
| Transaction creation (full 9-step flow) | End-to-end with services, employees, payments, commissions | Integration |
| Payroll period lifecycle | OPEN → CLOSED → PROCESSED, no re-processing | Integration |

### Tier 3: Standard (Test During Polish Phase)

| What | Why | Type |
|---|---|---|
| CRUD operations (services, employees, customers) | Standard but must validate correctly | Integration |
| Pagination and filtering | Edge cases with empty results, large datasets | Integration |
| Frontend form validation | Required fields, phone format, plate format | Component |
| POS transaction screen flow | Service selection → employee assignment → payment | E2E |
| Dashboard KPI calculations | Correct aggregations | Integration |
| SMS/Email delivery | Correct routing, template rendering | Unit |
| Notification preferences | Respect user opt-out settings | Unit |

---

## Test Project Structure

```
tests/
├── SplashSphere.Domain.Tests/
│   ├── Entities/
│   │   ├── TransactionTests.cs
│   │   ├── CommissionCalculationTests.cs
│   │   ├── PayrollEntryTests.cs
│   │   └── CashierShiftTests.cs
│   └── ValueObjects/
│       ├── PhoneNumberTests.cs
│       └── PlateNumberTests.cs
│
├── SplashSphere.Application.Tests/
│   ├── Features/
│   │   ├── Transactions/
│   │   │   ├── CreateTransactionHandlerTests.cs
│   │   │   └── OfflineSyncTransactionHandlerTests.cs
│   │   ├── Payroll/
│   │   │   └── ProcessPayrollHandlerTests.cs
│   │   ├── Inventory/
│   │   │   └── RecordStockMovementHandlerTests.cs
│   │   └── Shifts/
│   │       └── CloseShiftHandlerTests.cs
│   └── Validators/
│       └── CreateTransactionValidatorTests.cs
│
├── SplashSphere.API.Tests/
│   ├── Endpoints/
│   │   ├── TransactionEndpointTests.cs
│   │   ├── PayrollEndpointTests.cs
│   │   └── OnboardingEndpointTests.cs
│   ├── Middleware/
│   │   ├── TenantIsolationTests.cs
│   │   └── PlanEnforcementTests.cs
│   └── TestFixtures/
│       ├── TestWebApplicationFactory.cs
│       └── TestDataBuilder.cs
│
└── SplashSphere.Frontend.Tests/
    ├── components/
    │   ├── MoneyDisplay.test.tsx
    │   ├── StatusBadge.test.tsx
    │   └── PricingMatrixEditor.test.tsx
    └── hooks/
        ├── useOfflineTransaction.test.ts
        └── usePlan.test.ts
```

---

## Backend Testing Patterns

### Unit Test (Domain Logic)

```csharp
public class CommissionCalculationTests
{
    [Theory]
    [InlineData(CommissionType.PERCENTAGE, 220.00, 0.15, null, 3, 11.00)]
    [InlineData(CommissionType.FIXED_AMOUNT, 220.00, null, 45.00, 3, 15.00)]
    [InlineData(CommissionType.PERCENTAGE, 220.00, 0.15, null, 1, 33.00)]
    [InlineData(CommissionType.PERCENTAGE, 350.00, 0.12, null, 5, 8.40)]
    public void CalculateCommissionPerEmployee_ReturnsCorrectAmount(
        CommissionType type, decimal servicePrice, decimal? rate, 
        decimal? fixedAmount, int employeeCount, decimal expected)
    {
        var result = CommissionCalculator.Calculate(
            type, servicePrice, rate, fixedAmount, employeeCount);

        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateCommission_WithZeroEmployees_ThrowsException()
    {
        var act = () => CommissionCalculator.Calculate(
            CommissionType.PERCENTAGE, 220m, 0.15m, null, 0);

        act.Should().Throw<DomainException>()
            .WithMessage("*at least one employee*");
    }
}
```

### Handler Test (Application Layer — Mocked Dependencies)

```csharp
public class CreateTransactionHandlerTests
{
    private readonly ITransactionRepository _repo = Substitute.For<ITransactionRepository>();
    private readonly ITenantContext _tenant = Substitute.For<ITenantContext>();
    private readonly CreateTransactionHandler _handler;

    public CreateTransactionHandlerTests()
    {
        _tenant.TenantId.Returns("tenant-1");
        _handler = new CreateTransactionHandler(_repo, _tenant, ...);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesTransactionWithCorrectTotals()
    {
        var command = TestDataBuilder.CreateTransactionCommand(
            services: [new("svc-1", price: 220, quantity: 1, employees: ["emp-1", "emp-2"])]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalAmount.Should().Be(220m);
        result.Value.TotalCommissionAmount.Should().Be(33m); // 15% of 220
    }
}
```

### Integration Test (Real Database)

```csharp
public class TenantIsolationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _clientA;
    private readonly HttpClient _clientB;

    public TenantIsolationTests(TestWebApplicationFactory factory)
    {
        _clientA = factory.CreateClientForTenant("tenant-a");
        _clientB = factory.CreateClientForTenant("tenant-b");
    }

    [Fact]
    public async Task TenantA_CannotSee_TenantB_Services()
    {
        // Tenant A creates a service
        await _clientA.PostAsJsonAsync("/api/v1/services", 
            new { Name = "Secret Wash", BasePrice = 999 });

        // Tenant B lists services
        var response = await _clientB.GetFromJsonAsync<PagedResult<ServiceResponse>>(
            "/api/v1/services");

        response.Items.Should().NotContain(s => s.Name == "Secret Wash");
    }
}
```

### Test Web Application Factory

```csharp
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace PostgreSQL with Testcontainers
            services.RemoveAll<DbContextOptions<SplashSphereDbContext>>();
            services.AddDbContext<SplashSphereDbContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()));
        });
    }

    public HttpClient CreateClientForTenant(string tenantId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", GenerateTestJwt(tenantId));
        return client;
    }
}
```

---

## Test Data Conventions

Use Filipino test data for realism:

```csharp
public static class TestData
{
    // People
    public static string[] Names = ["Juan Dela Cruz", "Maria Santos", "Pedro Garcia", 
        "Ana Reyes", "Carlos Rivera", "Rosa Mendoza"];

    // Vehicles
    public static string[] Plates = ["ABC-1234", "DEF-5678", "GHI-9012"];
    public static string[] Makes = ["Toyota", "Honda", "Mitsubishi"];
    public static string[] Models = ["Vios", "City", "Xpander"];

    // Branches
    public static string[] Branches = ["SparkleWash - Makati", "SparkleWash - Cebu"];

    // Money (always in Philippine Peso)
    public static decimal BasicWashPrice = 220m;
    public static decimal PremiumWashPrice = 380m;
    public static decimal CommissionRate = 0.15m;
}
```

---

## Frontend Testing

### Component Test (Vitest + Testing Library)

```typescript
import { render, screen } from '@testing-library/react';
import { MoneyDisplay } from '@/components/MoneyDisplay';

test('formats peso amount correctly', () => {
  render(<MoneyDisplay amount={2999.5} />);
  expect(screen.getByText('₱2,999.50')).toBeInTheDocument();
});

test('shows negative amounts in red', () => {
  render(<MoneyDisplay amount={-150} />);
  const el = screen.getByText('-₱150.00');
  expect(el).toHaveClass('text-red-500');
});
```

### Hook Test (Offline Transaction)

```typescript
import { renderHook, act } from '@testing-library/react';
import { useCreateOfflineTransaction } from '@/hooks/use-create-offline-transaction';

test('creates transaction locally when offline', async () => {
  // Mock navigator.onLine = false
  vi.spyOn(navigator, 'onLine', 'get').mockReturnValue(false);

  const { result } = renderHook(() => useCreateOfflineTransaction());

  await act(async () => {
    const tx = await result.current.mutateAsync(mockTransactionData);
    expect(tx.syncStatus).toBe('pending');
    expect(tx.tempId).toBeTruthy();
  });
});
```

---

## Coverage Targets

| Layer | Target | Rationale |
|---|---|---|
| Domain (entities, calculations) | 90%+ | Pure logic, easy to test, highest business risk |
| Application (handlers, validators) | 80%+ | Business rules, validation logic |
| Infrastructure (repos, services) | 60%+ | EF Core queries tested via integration |
| API (endpoints) | 70%+ | Integration tests cover happy paths + error cases |
| Frontend (components) | 50%+ | Focus on business logic components, not layout |
| Frontend (hooks) | 70%+ | Data fetching and offline logic are critical |

---

## CI Integration

```yaml
# .github/workflows/test.yml
- name: Run backend tests
  run: dotnet test --configuration Release --logger "trx" --collect:"XPlat Code Coverage"

- name: Run frontend tests
  run: pnpm --filter admin test --coverage

- name: Check coverage
  run: |
    # Fail if domain layer < 90%
    # Fail if application layer < 80%
```

---

## Claude Code Prompt

```
Set up the testing infrastructure:

1. Create test projects: Domain.Tests, Application.Tests, API.Tests
2. Install: xUnit, FluentAssertions, NSubstitute, Bogus, Testcontainers.PostgreSql
3. Create TestWebApplicationFactory with Testcontainers PostgreSQL
4. Create TestDataBuilder with Filipino test data
5. Write Tier 1 tests:
   - CommissionCalculationTests (all 3 types × edge cases)
   - TransactionTotalTests (finalAmount formula)
   - PayrollCalculationTests (COMMISSION + DAILY)
   - CashAdvanceDeductionTests (boundary: can't go negative)
   - ShiftVarianceFormulaTests
   - TenantIsolationTests (Tenant A can't see Tenant B)
   - PlanEnforcementTests (Starter blocked from Growth features)
6. Frontend: set up Vitest + Testing Library in admin app
   - MoneyDisplay, StatusBadge component tests
   - formatPeso utility tests
```
