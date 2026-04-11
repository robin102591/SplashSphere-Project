# SplashSphere — API Documentation (OpenAPI / Swagger)

> **When:** Set up during Phase 1 (Foundation). Maintained by all agents throughout development.
> **Access:** `/docs` in Development/Staging only. Disabled in Production.

---

## Setup

### Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.*" />
```

### Program.cs
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SplashSphere API",
        Version = "v1",
        Description = "Multi-tenant car wash management platform API",
        Contact = new OpenApiContact
        {
            Name = "LezanobTech",
            Email = "dev@splashsphere.ph"
        }
    });

    // JWT Bearer auth
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Clerk JWT token. Format: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // XML comments
    var xmlPath = Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);

    options.TagActionsBy(api => new[] { api.GroupName ?? "Other" });
});

// Pipeline (dev only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SplashSphere API v1");
        c.RoutePrefix = "docs";
    });
}
```

Enable in `.csproj`:
```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<NoWarn>$(NoWarn);1591</NoWarn>
```

---

## Endpoint Documentation Pattern

```csharp
var group = app.MapGroup("/api/v1/services")
    .WithTags("Services")
    .RequireAuthorization();

group.MapGet("/", GetServices)
    .WithName("GetServices")
    .WithSummary("List all services")
    .WithDescription("Paginated list for the current tenant. Filter by category, active status.")
    .Produces<PagedResult<ServiceResponse>>(200)
    .Produces<ProblemDetails>(401);

group.MapPost("/", CreateService)
    .WithName("CreateService")
    .WithSummary("Create a new service")
    .Accepts<CreateServiceRequest>("application/json")
    .Produces<ServiceResponse>(201)
    .ProducesValidationProblem();
```

---

## Tag Groups

| Tag | Routes | Description |
|---|---|---|
| Auth & Onboarding | `/auth`, `/onboarding` | Sign-in, sign-up, onboarding |
| Branches | `/branches` | Branch CRUD |
| Services | `/services` | Service, pricing matrix, commission matrix |
| Packages | `/packages` | Service package CRUD |
| Customers | `/customers` | Customer CRUD |
| Vehicles | `/cars` | Vehicle CRUD, plate search |
| Employees | `/employees` | Employee CRUD, attendance |
| Transactions | `/transactions` | Transaction CRUD, offline sync |
| Queue | `/queue` | Queue management |
| Payroll | `/payroll` | Payroll periods, entries |
| Shifts | `/shifts` | Cashier shifts, cash movements |
| Inventory | `/supplies`, `/equipment`, `/purchase-orders`, `/suppliers` | All inventory |
| Reports | `/reports` | All reports |
| Billing | `/billing` | Subscription, payments |
| AI | `/ai` | Negosyo AI |
| Notifications | `/notifications` | Notifications, preferences |
| POS | `/pos` | Cache bundle |

---

## Response Conventions

```json
// Single item
{ "id": "...", "name": "..." }

// Paginated list
{ "items": [...], "totalCount": 100, "page": 1, "pageSize": 20 }

// Error (RFC 9457)
{ "type": "https://splashsphere.ph/errors/validation", "title": "Validation Failed",
  "status": 400, "errors": { "name": ["Required"] } }
```

**Common query params:** `page`, `pageSize`, `search`, `sortBy`, `sortOrder`, `branchId`, `isActive`

---

## Environment Access

| Environment | Swagger UI | OpenAPI JSON |
|---|---|---|
| Development | `/docs` (open) | `/swagger/v1/swagger.json` |
| Staging | `/docs` (basic auth) | Protected |
| Production | Disabled | Disabled |

---

## Claude Code Prompt

```
Set up OpenAPI/Swagger:
1. Install Swashbuckle packages
2. Configure in Program.cs with JWT auth, XML comments, tag groups
3. Enable XML docs in .csproj
4. Add .WithTags(), .WithName(), .WithSummary(), .Produces<T>() to ALL endpoints
5. Verify at /docs — all endpoints visible with try-it-out
```
