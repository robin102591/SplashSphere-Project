# SplashSphere — Data Migration Tool

> **Purpose:** Help new tenants import their existing business data (customers, vehicles, employees, services) from spreadsheets or other systems into SplashSphere.
> **When:** Post-launch. Build when tenants start asking "how do I import my existing customer list?"
> **Priority:** Nice-to-have for launch. Essential once you have 10+ tenants onboarding.

---

## The Problem

A car wash owner signs up for SplashSphere. They have:
- 200 regular customers in a notebook or Excel file
- 150 vehicles with plate numbers written down
- 8 employees with hire dates and rates in a spreadsheet
- A list of services with prices they've been using for years

Manually entering all of this into SplashSphere takes hours. Most owners won't do it — they'll start fresh and lose their customer history. A migration tool lets them upload a spreadsheet and import everything in minutes.

---

## What Can Be Imported

| Data Type | Source Format | Maps To | Complexity |
|---|---|---|---|
| Customers | CSV / Excel | Customer entity | Simple |
| Vehicles | CSV / Excel | Car entity (+ link to customer) | Medium (plate validation, make/model matching) |
| Employees | CSV / Excel | Employee entity | Simple |
| Services | CSV / Excel | Service entity (+ pricing matrix) | Medium (size-based pricing) |
| Transaction History | CSV / Excel | Transaction entity (read-only archive) | Complex (defer) |

---

## Import Flow

### Step 1: Upload

```
┌──────────────────────────────────────────────────────────┐
│  Import Data                                              │
│                                                          │
│  What do you want to import?                             │
│                                                          │
│  ○ Customers & Vehicles                                  │
│  ○ Employees                                             │
│  ○ Services & Pricing                                    │
│                                                          │
│  Upload your file:                                       │
│  ┌────────────────────────────────────────────────────┐  │
│  │  📄 Drop your .csv or .xlsx file here              │  │
│  │     or click to browse                             │  │
│  └────────────────────────────────────────────────────┘  │
│                                                          │
│  📥 [Download Template] — pre-formatted spreadsheet     │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

### Step 2: Column Mapping

After upload, the system auto-detects columns and lets the user map them:

```
┌──────────────────────────────────────────────────────────┐
│  Map Your Columns                                        │
│                                                          │
│  We found 5 columns in your file. Match them:           │
│                                                          │
│  Your Column          →  SplashSphere Field             │
│  ─────────────────────   ─────────────────────          │
│  "Customer Name"      →  [Name           ▾]             │
│  "Phone"              →  [Contact        ▾]             │
│  "Email Address"      →  [Email          ▾]             │
│  "Plate No."          →  [— Skip —       ▾]  (or Car)  │
│  "Address"            →  [Address        ▾]             │
│                                                          │
│  Preview (first 5 rows):                                │
│  ┌─────────────────┬──────────────┬───────────────────┐ │
│  │ Name            │ Contact      │ Email             │ │
│  │ Maria Santos    │ 09171234567  │ maria@gmail.com   │ │
│  │ Juan Reyes      │ 09189876543  │ —                 │ │
│  │ Pedro Garcia    │ 09201112233  │ pedro@yahoo.com   │ │
│  └─────────────────┴──────────────┴───────────────────┘ │
│                                                          │
│                          [← Back]  [Validate & Import →] │
└──────────────────────────────────────────────────────────┘
```

### Step 3: Validation

Before importing, validate all rows:

```
┌──────────────────────────────────────────────────────────┐
│  Validation Results                                      │
│                                                          │
│  200 rows found                                          │
│  ✅ 187 valid — ready to import                          │
│  ⚠️  8 warnings — will import with corrections           │
│  ❌  5 errors — will be skipped                           │
│                                                          │
│  Errors:                                                 │
│  Row 23: Phone "1234" is not a valid PH number — skip?  │
│  Row 45: Duplicate plate "ABC-1234" already exists       │
│  Row 67: Name is empty — required                        │
│  Row 89: Phone "09171234567" already exists (Maria S.)   │
│  Row 102: Plate "XY-123" invalid format                  │
│                                                          │
│  [Fix in spreadsheet & re-upload]  [Import 195 valid →]  │
└──────────────────────────────────────────────────────────┘
```

### Step 4: Import & Summary

```
┌──────────────────────────────────────────────────────────┐
│  ✅ Import Complete!                                      │
│                                                          │
│  Imported:   187 customers                               │
│  Corrected:    8 (phone numbers normalized)              │
│  Skipped:      5 (see error log)                         │
│                                                          │
│  [View Imported Customers]  [Import More]  [Done]        │
└──────────────────────────────────────────────────────────┘
```

---

## Download Templates

Pre-formatted spreadsheets the owner can fill in:

### customers_template.csv
```csv
Name,Contact,Email,Address
Maria Santos,09171234567,maria@gmail.com,"123 Rizal St, Makati"
Juan Reyes,09189876543,,
```

### vehicles_template.csv
```csv
CustomerName,CustomerContact,PlateNumber,Make,Model,VehicleType,Size,Color,Year
Maria Santos,09171234567,ABC-1234,Toyota,Vios,Sedan,Medium,White,2020
Juan Reyes,09189876543,DEF-5678,Mitsubishi,Xpander,SUV,Large,Black,2022
```

### employees_template.csv
```csv
Name,Type,DailyRate,DateHired
Juan Dela Cruz,COMMISSION,,2024-01-15
Ana Reyes,DAILY,500,2023-06-01
```

### services_template.csv
```csv
Name,Category,BasePrice,DurationMinutes,Description
Basic Wash,Basic Services,200,30,Standard exterior wash
Premium Wash,Premium Services,350,45,Full exterior with foam
```

---

## Validation Rules

| Field | Validation |
|---|---|
| Customer Name | Required, 2-100 chars |
| Contact | Philippine phone format (09XX...), unique within tenant |
| Email | Valid email if provided, unique within tenant |
| Plate Number | PH format (ABC-1234 or ABC-12CD), unique within tenant |
| Make | Fuzzy match against existing makes (Toyota, TOYOTA, toyota → Toyota) |
| Model | Create if not exists, linked to matched make |
| Vehicle Type | Match against: Sedan, SUV, Van, Truck, Hatchback, Pickup |
| Size | Match against: Small, Medium, Large, XL |
| Employee Type | Must be COMMISSION or DAILY |
| Daily Rate | Required if type = DAILY, must be > 0 |
| Date Hired | Valid date, not in the future |
| Service Base Price | Required, > 0 |

---

## Backend

```csharp
public interface IDataMigrationService
{
    Task<ValidationResult> ValidateImport(ImportType type, Stream fileStream, 
        ColumnMapping mapping);
    Task<ImportResult> ExecuteImport(ImportType type, Stream fileStream, 
        ColumnMapping mapping);
    byte[] GenerateTemplate(ImportType type);
}

public enum ImportType { Customers, Vehicles, Employees, Services }

public sealed record ValidationResult
{
    public int TotalRows { get; init; }
    public int ValidRows { get; init; }
    public int WarningRows { get; init; }
    public int ErrorRows { get; init; }
    public List<ImportError> Errors { get; init; } = [];
    public List<ImportWarning> Warnings { get; init; } = [];
}

public sealed record ImportResult
{
    public int Imported { get; init; }
    public int Corrected { get; init; }
    public int Skipped { get; init; }
    public List<ImportError> SkippedErrors { get; init; } = [];
}
```

### API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/import/templates/{type}` | Download CSV/Excel template |
| `POST` | `/import/validate` | Upload file + column mapping → validation results |
| `POST` | `/import/execute` | Execute validated import |

### File Parsing

Use **CsvHelper** for CSV and **ClosedXML** for Excel files:
```xml
<PackageReference Include="CsvHelper" Version="33.*" />
<PackageReference Include="ClosedXML" Version="0.104.*" />
```

---

## Frontend

### Route: `/settings/import`

Add to admin settings:
- "Import Data" section with cards for each import type
- Upload wizard (4-step flow above)
- Download template buttons

---

## Claude Code Prompt

```
Build the data migration/import tool:

Application/Services/DataMigrationService.cs:
- ValidateImport: parse file, apply validation rules, return results
- ExecuteImport: create entities in bulk (batch insert)
- GenerateTemplate: create CSV with headers and example rows

Use CsvHelper for CSV, ClosedXML for Excel parsing.

Validation: phone normalization, plate format check, fuzzy make matching,
duplicate detection against existing tenant data.

Import is transactional: if any critical error, roll back entire batch.
Warnings (correctable) are auto-fixed and imported.

Endpoints: ImportEndpoints.cs
- GET /import/templates/{type} (customers/vehicles/employees/services)
- POST /import/validate (multipart file + JSON column mapping)
- POST /import/execute

Frontend: /settings/import page with upload wizard,
column mapping UI, validation results, and import summary.
```
