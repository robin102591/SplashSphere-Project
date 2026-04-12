using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Import;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.Services;

public sealed class DataMigrationService(
    IApplicationDbContext db,
    ITenantContext tenantContext) : IDataMigrationService
{
    // ── Header alias mappings for fuzzy column detection ─────────────────────

    private static readonly Dictionary<string, string[]> CustomerAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Name"] = ["name", "customer name", "customer", "full name", "fullname"],
        ["Contact"] = ["contact", "contact number", "contactnumber", "phone", "phone number", "phonenumber", "mobile", "mobile number", "cell", "cellphone"],
        ["Email"] = ["email", "email address", "emailaddress", "e-mail"],
        ["Address"] = ["address", "addr", "location"],
    };

    private static readonly Dictionary<string, string[]> VehicleAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PlateNumber"] = ["plate number", "platenumber", "plate no", "plate no.", "plate", "license plate", "plate #"],
        ["VehicleType"] = ["vehicle type", "vehicletype", "type", "car type"],
        ["Size"] = ["size", "vehicle size", "vehiclesize"],
        ["Make"] = ["make", "brand", "manufacturer"],
        ["Model"] = ["model", "car model"],
        ["Color"] = ["color", "colour", "car color"],
        ["Year"] = ["year", "model year", "yr"],
        ["CustomerName"] = ["customer name", "customername", "customer", "owner", "owner name"],
        ["CustomerContact"] = ["customer contact", "customercontact", "owner contact", "owner phone"],
    };

    private static readonly Dictionary<string, string[]> EmployeeAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Name"] = ["name", "employee name", "employeename", "full name", "fullname"],
        ["Type"] = ["type", "employee type", "employeetype", "compensation", "pay type"],
        ["DailyRate"] = ["daily rate", "dailyrate", "rate", "daily pay", "wage"],
        ["DateHired"] = ["date hired", "datehired", "hired date", "hireddate", "hire date", "start date", "startdate"],
        ["Contact"] = ["contact", "contact number", "contactnumber", "phone", "mobile"],
        ["Email"] = ["email", "email address"],
    };

    private static readonly Dictionary<string, string[]> ServiceAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Name"] = ["name", "service name", "servicename", "service"],
        ["Category"] = ["category", "service category", "servicecategory", "cat"],
        ["BasePrice"] = ["base price", "baseprice", "price", "rate", "cost"],
        ["DurationMinutes"] = ["duration", "durationminutes", "duration minutes", "duration (min)", "minutes", "time", "mins"],
        ["Description"] = ["description", "desc", "details", "notes"],
    };

    // ── Philippine plate number regex ────────────────────────────────────────

    private static readonly Regex PhPlateRegex = new(
        @"^[A-Z]{2,3}\s?-?\s?\d{4}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // ── Public interface ─────────────────────────────────────────────────────

    public async Task<ImportValidationResult> DetectColumnsAsync(
        ImportType type, Stream fileStream, string fileName, CancellationToken ct)
    {
        var rows = ParseFile(fileStream, fileName);
        var detectedColumns = rows.Count > 0
            ? rows[0].Keys.ToList()
            : [];

        var aliases = GetAliasesForType(type);
        var suggestedMapping = new Dictionary<string, string>();

        foreach (var col in detectedColumns)
        {
            foreach (var (target, synonyms) in aliases)
            {
                if (synonyms.Any(s => string.Equals(s, col.Trim(), StringComparison.OrdinalIgnoreCase))
                    || string.Equals(target, col.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    suggestedMapping[col] = target;
                    break;
                }
            }
        }

        var previewRows = rows.Take(5).ToList();

        return new ImportValidationResult
        {
            TotalRows = rows.Count,
            ValidRows = rows.Count,
            DetectedColumns = detectedColumns,
            PreviewRows = previewRows,
        };
    }

    public async Task<ImportValidationResult> ValidateAsync(
        ImportType type, Stream fileStream, string fileName, ColumnMapping mapping, CancellationToken ct)
    {
        var rows = ParseFile(fileStream, fileName);
        var mapped = ApplyMapping(rows, mapping);

        return type switch
        {
            ImportType.Customers => await ValidateCustomersAsync(mapped, ct),
            ImportType.Vehicles => await ValidateVehiclesAsync(mapped, ct),
            ImportType.Employees => await ValidateEmployeesAsync(mapped, ct),
            ImportType.Services => await ValidateServicesAsync(mapped, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }

    public async Task<ImportResult> ExecuteAsync(
        ImportType type, Stream fileStream, string fileName, ColumnMapping mapping, CancellationToken ct)
    {
        var rows = ParseFile(fileStream, fileName);
        var mapped = ApplyMapping(rows, mapping);

        return type switch
        {
            ImportType.Customers => await ExecuteCustomersAsync(mapped, ct),
            ImportType.Vehicles => await ExecuteVehiclesAsync(mapped, ct),
            ImportType.Employees => await ExecuteEmployeesAsync(mapped, ct),
            ImportType.Services => await ExecuteServicesAsync(mapped, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }

    public byte[] GenerateTemplate(ImportType type)
    {
        var (headers, exampleRows) = type switch
        {
            ImportType.Customers => (
                "Name,Contact,Email,Address",
                new[]
                {
                    "Juan Dela Cruz,09171234567,juan@email.com,123 Rizal St Makati",
                    "Maria Santos,09181234567,maria@email.com,456 Bonifacio Ave BGC",
                    "Pedro Reyes,09191234567,,789 Mabini St Pasig",
                }),
            ImportType.Vehicles => (
                "CustomerName,CustomerContact,PlateNumber,Make,Model,VehicleType,Size,Color,Year",
                new[]
                {
                    "Juan Dela Cruz,09171234567,ABC-1234,Toyota,Vios,Sedan,Medium,White,2020",
                    "Maria Santos,09181234567,XYZ-5678,Honda,CR-V,SUV,Large,Black,2022",
                    ",,DEF-9012,Mitsubishi,L300,Van,Large,Silver,2018",
                }),
            ImportType.Employees => (
                "Name,Type,DailyRate,DateHired",
                new[]
                {
                    "Carlos Garcia,Commission,,2024-01-15",
                    "Ana Reyes,Daily,600,2024-03-01",
                    "Roberto Santos,Hybrid,400,2024-06-15",
                }),
            ImportType.Services => (
                "Name,Category,BasePrice,DurationMinutes,Description",
                new[]
                {
                    "Basic Exterior Wash,Wash,150,30,Quick exterior rinse and dry",
                    "Full Detail,Detail,500,90,Complete interior and exterior detail",
                    "Engine Wash,Specialty,300,45,Engine bay cleaning and degreasing",
                }),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

        var sb = new StringBuilder();
        sb.AppendLine(headers);
        foreach (var row in exampleRows)
            sb.AppendLine(row);

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    // ── File parsing ─────────────────────────────────────────────────────────

    private static List<Dictionary<string, string>> ParseFile(Stream stream, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext is ".xlsx" or ".xls"
            ? ParseExcel(stream)
            : ParseCsv(stream);
    }

    private static List<Dictionary<string, string>> ParseExcel(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var rows = new List<Dictionary<string, string>>();

        var headerRow = worksheet.Row(1);
        var headers = new List<string>();
        var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

        for (var col = 1; col <= lastCol; col++)
        {
            var val = headerRow.Cell(col).GetString().Trim();
            headers.Add(val);
        }

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        for (var row = 2; row <= lastRow; row++)
        {
            var wsRow = worksheet.Row(row);
            // Skip completely empty rows
            if (wsRow.IsEmpty()) continue;

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var col = 0; col < headers.Count; col++)
            {
                dict[headers[col]] = wsRow.Cell(col + 1).GetString().Trim();
            }

            // Skip rows where all values are empty
            if (dict.Values.All(string.IsNullOrWhiteSpace)) continue;

            rows.Add(dict);
        }

        return rows;
    }

    private static List<Dictionary<string, string>> ParseCsv(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim,
        });

        var rows = new List<Dictionary<string, string>>();
        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? [];

        while (csv.Read())
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in headers)
            {
                dict[header] = csv.GetField(header)?.Trim() ?? string.Empty;
            }

            if (dict.Values.All(string.IsNullOrWhiteSpace)) continue;

            rows.Add(dict);
        }

        return rows;
    }

    // ── Mapping helper ───────────────────────────────────────────────────────

    private static List<Dictionary<string, string>> ApplyMapping(
        List<Dictionary<string, string>> rows,
        ColumnMapping mapping)
    {
        return rows.Select(row =>
        {
            var mapped = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (sourceCol, targetCol) in mapping.Mappings)
            {
                if (row.TryGetValue(sourceCol, out var value))
                    mapped[targetCol] = value;
            }
            return mapped;
        }).ToList();
    }

    // ── Validation: Customers ────────────────────────────────────────────────

    private async Task<ImportValidationResult> ValidateCustomersAsync(
        List<Dictionary<string, string>> rows, CancellationToken ct)
    {
        var errors = new List<ImportRowError>();
        var warnings = new List<ImportRowWarning>();
        var tenantId = tenantContext.TenantId;

        var existingEmails = await db.Customers
            .Where(c => c.Email != null)
            .Select(c => c.Email!.ToLower())
            .ToListAsync(ct);
        var emailSet = existingEmails.ToHashSet();

        var existingContacts = await db.Customers
            .Where(c => c.ContactNumber != null)
            .Select(c => c.ContactNumber!)
            .ToListAsync(ct);
        var contactSet = existingContacts.ToHashSet();

        var errorRowSet = new HashSet<int>();
        var warningRowSet = new HashSet<int>();

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 1;
            var row = rows[i];

            var name = GetValue(row, "Name");
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(new ImportRowError(rowNum, "Name", "Name is required."));
                errorRowSet.Add(rowNum);
                continue;
            }

            var contact = GetValue(row, "Contact");
            if (!string.IsNullOrWhiteSpace(contact))
            {
                var normalized = NormalizePhPhone(contact);
                if (normalized is null)
                {
                    errors.Add(new ImportRowError(rowNum, "Contact", $"Invalid PH phone number: '{contact}'. Expected 09XXXXXXXXX format."));
                    errorRowSet.Add(rowNum);
                }
                else
                {
                    if (normalized != contact.Trim())
                        warnings.Add(new ImportRowWarning(rowNum, "Contact", $"Phone normalized from '{contact}'.", normalized));

                    if (contactSet.Contains(normalized))
                    {
                        warnings.Add(new ImportRowWarning(rowNum, "Contact", $"Contact '{normalized}' already exists. Row will be skipped as duplicate.", null));
                        warningRowSet.Add(rowNum);
                    }
                }
            }

            var email = GetValue(row, "Email");
            if (!string.IsNullOrWhiteSpace(email))
            {
                if (!IsValidEmail(email))
                {
                    errors.Add(new ImportRowError(rowNum, "Email", $"Invalid email format: '{email}'."));
                    errorRowSet.Add(rowNum);
                }
                else if (emailSet.Contains(email.ToLowerInvariant()))
                {
                    warnings.Add(new ImportRowWarning(rowNum, "Email", $"Email '{email}' already exists. Row will be skipped as duplicate.", null));
                    warningRowSet.Add(rowNum);
                }
            }
        }

        return new ImportValidationResult
        {
            TotalRows = rows.Count,
            ValidRows = rows.Count - errorRowSet.Count,
            WarningRows = warningRowSet.Count,
            ErrorRows = errorRowSet.Count,
            Errors = errors,
            Warnings = warnings,
        };
    }

    // ── Validation: Vehicles ─────────────────────────────────────────────────

    private async Task<ImportValidationResult> ValidateVehiclesAsync(
        List<Dictionary<string, string>> rows, CancellationToken ct)
    {
        var errors = new List<ImportRowError>();
        var warnings = new List<ImportRowWarning>();

        var vehicleTypes = await db.VehicleTypes.ToListAsync(ct);
        var sizes = await db.Sizes.ToListAsync(ct);
        var makes = await db.Makes.ToListAsync(ct);
        var models = await db.Models.ToListAsync(ct);
        var existingPlates = (await db.Cars.Select(c => c.PlateNumber).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var defaultVehicleType = vehicleTypes.FirstOrDefault();
        var defaultSize = sizes.FirstOrDefault();

        var errorRowSet = new HashSet<int>();
        var warningRowSet = new HashSet<int>();

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 1;
            var row = rows[i];

            var plate = GetValue(row, "PlateNumber");
            if (string.IsNullOrWhiteSpace(plate))
            {
                errors.Add(new ImportRowError(rowNum, "PlateNumber", "Plate number is required."));
                errorRowSet.Add(rowNum);
                continue;
            }

            var normalizedPlate = plate.ToUpperInvariant().Replace(" ", "");
            if (!PhPlateRegex.IsMatch(normalizedPlate))
            {
                errors.Add(new ImportRowError(rowNum, "PlateNumber", $"Invalid PH plate format: '{plate}'. Expected format: ABC-1234."));
                errorRowSet.Add(rowNum);
                continue;
            }

            if (existingPlates.Contains(normalizedPlate))
            {
                errors.Add(new ImportRowError(rowNum, "PlateNumber", $"Plate '{normalizedPlate}' already exists in the system."));
                errorRowSet.Add(rowNum);
                continue;
            }

            var vtName = GetValue(row, "VehicleType");
            if (!string.IsNullOrWhiteSpace(vtName))
            {
                var matched = vehicleTypes.FirstOrDefault(v =>
                    string.Equals(v.Name, vtName, StringComparison.OrdinalIgnoreCase));
                if (matched is null && defaultVehicleType is not null)
                {
                    warnings.Add(new ImportRowWarning(rowNum, "VehicleType",
                        $"Vehicle type '{vtName}' not found. Defaulting to '{defaultVehicleType.Name}'.",
                        defaultVehicleType.Name));
                    warningRowSet.Add(rowNum);
                }
            }
            else if (defaultVehicleType is not null)
            {
                warnings.Add(new ImportRowWarning(rowNum, "VehicleType",
                    $"Vehicle type not provided. Defaulting to '{defaultVehicleType.Name}'.",
                    defaultVehicleType.Name));
                warningRowSet.Add(rowNum);
            }

            var sizeName = GetValue(row, "Size");
            if (!string.IsNullOrWhiteSpace(sizeName))
            {
                var matched = sizes.FirstOrDefault(s =>
                    string.Equals(s.Name, sizeName, StringComparison.OrdinalIgnoreCase));
                if (matched is null && defaultSize is not null)
                {
                    warnings.Add(new ImportRowWarning(rowNum, "Size",
                        $"Size '{sizeName}' not found. Defaulting to '{defaultSize.Name}'.",
                        defaultSize.Name));
                    warningRowSet.Add(rowNum);
                }
            }
            else if (defaultSize is not null)
            {
                warnings.Add(new ImportRowWarning(rowNum, "Size",
                    $"Size not provided. Defaulting to '{defaultSize.Name}'.",
                    defaultSize.Name));
                warningRowSet.Add(rowNum);
            }

            var makeName = GetValue(row, "Make");
            if (!string.IsNullOrWhiteSpace(makeName))
            {
                var matchedMake = makes.FirstOrDefault(m =>
                    string.Equals(m.Name, makeName, StringComparison.OrdinalIgnoreCase));
                if (matchedMake is null)
                {
                    warnings.Add(new ImportRowWarning(rowNum, "Make",
                        $"Make '{makeName}' not found in the system.", null));
                    warningRowSet.Add(rowNum);
                }
                else
                {
                    var modelName = GetValue(row, "Model");
                    if (!string.IsNullOrWhiteSpace(modelName))
                    {
                        var matchedModel = models.FirstOrDefault(m =>
                            m.MakeId == matchedMake.Id &&
                            string.Equals(m.Name, modelName, StringComparison.OrdinalIgnoreCase));
                        if (matchedModel is null)
                        {
                            warnings.Add(new ImportRowWarning(rowNum, "Model",
                                $"Model '{modelName}' not found for make '{matchedMake.Name}'.", null));
                            warningRowSet.Add(rowNum);
                        }
                    }
                }
            }
        }

        return new ImportValidationResult
        {
            TotalRows = rows.Count,
            ValidRows = rows.Count - errorRowSet.Count,
            WarningRows = warningRowSet.Count,
            ErrorRows = errorRowSet.Count,
            Errors = errors,
            Warnings = warnings,
        };
    }

    // ── Validation: Employees ────────────────────────────────────────────────

    private async Task<ImportValidationResult> ValidateEmployeesAsync(
        List<Dictionary<string, string>> rows, CancellationToken ct)
    {
        var errors = new List<ImportRowError>();
        var warnings = new List<ImportRowWarning>();

        var branches = await db.Branches.Where(b => b.IsActive).ToListAsync(ct);
        var defaultBranch = branches.FirstOrDefault();

        var errorRowSet = new HashSet<int>();
        var warningRowSet = new HashSet<int>();

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 1;
            var row = rows[i];

            var name = GetValue(row, "Name");
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(new ImportRowError(rowNum, "Name", "Name is required."));
                errorRowSet.Add(rowNum);
                continue;
            }

            var typeStr = GetValue(row, "Type");
            if (string.IsNullOrWhiteSpace(typeStr))
            {
                errors.Add(new ImportRowError(rowNum, "Type", "Employee type is required (Commission, Daily, or Hybrid)."));
                errorRowSet.Add(rowNum);
                continue;
            }

            if (!TryParseEmployeeType(typeStr, out var empType))
            {
                errors.Add(new ImportRowError(rowNum, "Type", $"Invalid employee type: '{typeStr}'. Expected Commission, Daily, or Hybrid."));
                errorRowSet.Add(rowNum);
                continue;
            }

            if (empType is EmployeeType.Daily or EmployeeType.Hybrid)
            {
                var rateStr = GetValue(row, "DailyRate");
                if (string.IsNullOrWhiteSpace(rateStr) || !decimal.TryParse(rateStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate) || rate <= 0)
                {
                    errors.Add(new ImportRowError(rowNum, "DailyRate", $"Daily rate is required and must be > 0 for {empType} employees."));
                    errorRowSet.Add(rowNum);
                }
            }

            var dateStr = GetValue(row, "DateHired");
            if (!string.IsNullOrWhiteSpace(dateStr))
            {
                if (!TryParseDate(dateStr, out var parsedDate))
                {
                    errors.Add(new ImportRowError(rowNum, "DateHired", $"Invalid date format: '{dateStr}'. Use yyyy-MM-dd or MM/dd/yyyy."));
                    errorRowSet.Add(rowNum);
                }
                else if (parsedDate > DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    errors.Add(new ImportRowError(rowNum, "DateHired", "Hire date cannot be in the future."));
                    errorRowSet.Add(rowNum);
                }
            }

            if (defaultBranch is null)
            {
                errors.Add(new ImportRowError(rowNum, "Branch", "No active branch found in tenant. Create a branch first."));
                errorRowSet.Add(rowNum);
            }
        }

        return new ImportValidationResult
        {
            TotalRows = rows.Count,
            ValidRows = rows.Count - errorRowSet.Count,
            WarningRows = warningRowSet.Count,
            ErrorRows = errorRowSet.Count,
            Errors = errors,
            Warnings = warnings,
        };
    }

    // ── Validation: Services ─────────────────────────────────────────────────

    private async Task<ImportValidationResult> ValidateServicesAsync(
        List<Dictionary<string, string>> rows, CancellationToken ct)
    {
        var errors = new List<ImportRowError>();
        var warnings = new List<ImportRowWarning>();

        var categories = await db.ServiceCategories.ToListAsync(ct);
        var defaultCategory = categories.FirstOrDefault();

        var errorRowSet = new HashSet<int>();
        var warningRowSet = new HashSet<int>();

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 1;
            var row = rows[i];

            var name = GetValue(row, "Name");
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(new ImportRowError(rowNum, "Name", "Service name is required."));
                errorRowSet.Add(rowNum);
                continue;
            }

            var priceStr = GetValue(row, "BasePrice");
            if (string.IsNullOrWhiteSpace(priceStr) || !decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price <= 0)
            {
                errors.Add(new ImportRowError(rowNum, "BasePrice", "Base price is required and must be > 0."));
                errorRowSet.Add(rowNum);
                continue;
            }

            var catName = GetValue(row, "Category");
            if (!string.IsNullOrWhiteSpace(catName))
            {
                var matched = categories.FirstOrDefault(c =>
                    string.Equals(c.Name, catName, StringComparison.OrdinalIgnoreCase));
                if (matched is null && defaultCategory is not null)
                {
                    warnings.Add(new ImportRowWarning(rowNum, "Category",
                        $"Category '{catName}' not found. Defaulting to '{defaultCategory.Name}'.",
                        defaultCategory.Name));
                    warningRowSet.Add(rowNum);
                }
            }
            else if (defaultCategory is not null)
            {
                warnings.Add(new ImportRowWarning(rowNum, "Category",
                    $"Category not provided. Defaulting to '{defaultCategory.Name}'.",
                    defaultCategory.Name));
                warningRowSet.Add(rowNum);
            }
            else
            {
                errors.Add(new ImportRowError(rowNum, "Category", "No service categories found in tenant. Create a category first."));
                errorRowSet.Add(rowNum);
            }

            var durStr = GetValue(row, "DurationMinutes");
            if (!string.IsNullOrWhiteSpace(durStr) && (!int.TryParse(durStr, out var dur) || dur <= 0))
            {
                warnings.Add(new ImportRowWarning(rowNum, "DurationMinutes",
                    $"Invalid duration '{durStr}'. Defaulting to 30 minutes.", "30"));
                warningRowSet.Add(rowNum);
            }
        }

        return new ImportValidationResult
        {
            TotalRows = rows.Count,
            ValidRows = rows.Count - errorRowSet.Count,
            WarningRows = warningRowSet.Count,
            ErrorRows = errorRowSet.Count,
            Errors = errors,
            Warnings = warnings,
        };
    }

    // ── Execute: Customers ───────────────────────────────────────────────────

    private async Task<ImportResult> ExecuteCustomersAsync(
        List<Dictionary<string, string>> rows, CancellationToken ct)
    {
        var validation = await ValidateCustomersAsync(rows, ct);
        var errorRows = validation.Errors.Select(e => e.Row).ToHashSet();

        // Also collect duplicate-warning rows (which should be skipped)
        var duplicateRows = validation.Warnings
            .Where(w => w.Message.Contains("already exists"))
            .Select(w => w.Row)
            .ToHashSet();

        var imported = 0;
        var corrected = 0;
        var tenantId = tenantContext.TenantId;

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 1;
            if (errorRows.Contains(rowNum) || duplicateRows.Contains(rowNum)) continue;

            var row = rows[i];
            var (firstName, lastName) = SplitName(GetValue(row, "Name")!);
            var contact = NormalizePhPhone(GetValue(row, "Contact"));
            var email = GetValue(row, "Email");

            var customer = new Customer(tenantId, firstName, lastName, email, contact);

            db.Customers.Add(customer);
            imported++;

            if (validation.Warnings.Any(w => w.Row == rowNum))
                corrected++;
        }

        if (imported > 0)
            await db.SaveChangesAsync(ct);

        return new ImportResult
        {
            Imported = imported,
            Corrected = corrected,
            Skipped = rows.Count - imported,
            SkippedErrors = validation.Errors,
        };
    }

    // ── Execute: Vehicles ────────────────────────────────────────────────────

    private async Task<ImportResult> ExecuteVehiclesAsync(
        List<Dictionary<string, string>> rows, CancellationToken ct)
    {
        var validation = await ValidateVehiclesAsync(rows, ct);
        var errorRows = validation.Errors.Select(e => e.Row).ToHashSet();

        var vehicleTypes = await db.VehicleTypes.ToListAsync(ct);
        var sizes = await db.Sizes.ToListAsync(ct);
        var makes = await db.Makes.ToListAsync(ct);
        var models = await db.Models.ToListAsync(ct);
        var customers = await db.Customers.ToListAsync(ct);

        var defaultVehicleType = vehicleTypes.FirstOrDefault();
        var defaultSize = sizes.FirstOrDefault();
        var tenantId = tenantContext.TenantId;

        var imported = 0;
        var corrected = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 1;
            if (errorRows.Contains(rowNum)) continue;

            var row = rows[i];
            var plate = GetValue(row, "PlateNumber")!.ToUpperInvariant().Replace(" ", "");

            // Resolve vehicle type
            var vtName = GetValue(row, "VehicleType");
            var vt = (!string.IsNullOrWhiteSpace(vtName)
                ? vehicleTypes.FirstOrDefault(v => string.Equals(v.Name, vtName, StringComparison.OrdinalIgnoreCase))
                : null) ?? defaultVehicleType;

            if (vt is null) continue; // Should not happen if validation passed

            // Resolve size
            var sizeName = GetValue(row, "Size");
            var size = (!string.IsNullOrWhiteSpace(sizeName)
                ? sizes.FirstOrDefault(s => string.Equals(s.Name, sizeName, StringComparison.OrdinalIgnoreCase))
                : null) ?? defaultSize;

            if (size is null) continue;

            // Resolve make and model
            var makeName = GetValue(row, "Make");
            var matchedMake = !string.IsNullOrWhiteSpace(makeName)
                ? makes.FirstOrDefault(m => string.Equals(m.Name, makeName, StringComparison.OrdinalIgnoreCase))
                : null;

            var modelName = GetValue(row, "Model");
            var matchedModel = matchedMake is not null && !string.IsNullOrWhiteSpace(modelName)
                ? models.FirstOrDefault(m => m.MakeId == matchedMake.Id &&
                    string.Equals(m.Name, modelName, StringComparison.OrdinalIgnoreCase))
                : null;

            // Resolve customer
            string? customerId = null;
            var custName = GetValue(row, "CustomerName");
            var custContact = GetValue(row, "CustomerContact");

            if (!string.IsNullOrWhiteSpace(custContact))
            {
                var normalizedContact = NormalizePhPhone(custContact);
                if (normalizedContact is not null)
                {
                    var cust = customers.FirstOrDefault(c => c.ContactNumber == normalizedContact);
                    customerId = cust?.Id;
                }
            }

            if (customerId is null && !string.IsNullOrWhiteSpace(custName))
            {
                var cust = customers.FirstOrDefault(c =>
                    string.Equals(c.FullName, custName.Trim(), StringComparison.OrdinalIgnoreCase));
                customerId = cust?.Id;
            }

            var color = GetValue(row, "Color");
            int? year = null;
            var yearStr = GetValue(row, "Year");
            if (!string.IsNullOrWhiteSpace(yearStr) && int.TryParse(yearStr, out var parsedYear))
                year = parsedYear;

            var car = new Car(tenantId, vt.Id, size.Id, plate, customerId, matchedMake?.Id, matchedModel?.Id, color, year);
            db.Cars.Add(car);
            imported++;

            if (validation.Warnings.Any(w => w.Row == rowNum))
                corrected++;
        }

        if (imported > 0)
            await db.SaveChangesAsync(ct);

        return new ImportResult
        {
            Imported = imported,
            Corrected = corrected,
            Skipped = rows.Count - imported,
            SkippedErrors = validation.Errors,
        };
    }

    // ── Execute: Employees ───────────────────────────────────────────────────

    private async Task<ImportResult> ExecuteEmployeesAsync(
        List<Dictionary<string, string>> rows, CancellationToken ct)
    {
        var validation = await ValidateEmployeesAsync(rows, ct);
        var errorRows = validation.Errors.Select(e => e.Row).ToHashSet();

        var branches = await db.Branches.Where(b => b.IsActive).ToListAsync(ct);
        var defaultBranch = branches.FirstOrDefault();
        var tenantId = tenantContext.TenantId;

        if (defaultBranch is null)
        {
            return new ImportResult
            {
                Imported = 0,
                Skipped = rows.Count,
                SkippedErrors = validation.Errors,
            };
        }

        var imported = 0;
        var corrected = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 1;
            if (errorRows.Contains(rowNum)) continue;

            var row = rows[i];
            var (firstName, lastName) = SplitName(GetValue(row, "Name")!);
            TryParseEmployeeType(GetValue(row, "Type")!, out var empType);

            decimal? dailyRate = null;
            var rateStr = GetValue(row, "DailyRate");
            if (!string.IsNullOrWhiteSpace(rateStr) && decimal.TryParse(rateStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate))
                dailyRate = rate;

            DateOnly? hiredDate = null;
            var dateStr = GetValue(row, "DateHired");
            if (!string.IsNullOrWhiteSpace(dateStr) && TryParseDate(dateStr, out var parsed))
                hiredDate = parsed;

            var email = GetValue(row, "Email");
            var contact = NormalizePhPhone(GetValue(row, "Contact"));

            var employee = new Employee(
                tenantId, defaultBranch.Id, firstName, lastName, empType,
                dailyRate, email, contact, hiredDate);

            db.Employees.Add(employee);
            imported++;

            if (validation.Warnings.Any(w => w.Row == rowNum))
                corrected++;
        }

        if (imported > 0)
            await db.SaveChangesAsync(ct);

        return new ImportResult
        {
            Imported = imported,
            Corrected = corrected,
            Skipped = rows.Count - imported,
            SkippedErrors = validation.Errors,
        };
    }

    // ── Execute: Services ────────────────────────────────────────────────────

    private async Task<ImportResult> ExecuteServicesAsync(
        List<Dictionary<string, string>> rows, CancellationToken ct)
    {
        var validation = await ValidateServicesAsync(rows, ct);
        var errorRows = validation.Errors.Select(e => e.Row).ToHashSet();

        var categories = await db.ServiceCategories.ToListAsync(ct);
        var defaultCategory = categories.FirstOrDefault();
        var tenantId = tenantContext.TenantId;

        if (defaultCategory is null)
        {
            return new ImportResult
            {
                Imported = 0,
                Skipped = rows.Count,
                SkippedErrors = validation.Errors,
            };
        }

        var imported = 0;
        var corrected = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 1;
            if (errorRows.Contains(rowNum)) continue;

            var row = rows[i];
            var name = GetValue(row, "Name")!;
            decimal.TryParse(GetValue(row, "BasePrice"), NumberStyles.Number, CultureInfo.InvariantCulture, out var basePrice);

            var catName = GetValue(row, "Category");
            var category = (!string.IsNullOrWhiteSpace(catName)
                ? categories.FirstOrDefault(c => string.Equals(c.Name, catName, StringComparison.OrdinalIgnoreCase))
                : null) ?? defaultCategory;

            var description = GetValue(row, "Description");

            var service = new Service(tenantId, category.Id, name, basePrice, description);

            db.Services.Add(service);
            imported++;

            if (validation.Warnings.Any(w => w.Row == rowNum))
                corrected++;
        }

        if (imported > 0)
            await db.SaveChangesAsync(ct);

        return new ImportResult
        {
            Imported = imported,
            Corrected = corrected,
            Skipped = rows.Count - imported,
            SkippedErrors = validation.Errors,
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Dictionary<string, string[]> GetAliasesForType(ImportType type) => type switch
    {
        ImportType.Customers => CustomerAliases,
        ImportType.Vehicles => VehicleAliases,
        ImportType.Employees => EmployeeAliases,
        ImportType.Services => ServiceAliases,
        _ => new Dictionary<string, string[]>(),
    };

    private static string? GetValue(Dictionary<string, string> row, string key)
    {
        return row.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : null;
    }

    private static (string FirstName, string LastName) SplitName(string fullName)
    {
        var trimmed = fullName.Trim();
        var lastSpace = trimmed.LastIndexOf(' ');
        if (lastSpace <= 0)
            return (trimmed, string.Empty);

        return (trimmed[..lastSpace].Trim(), trimmed[(lastSpace + 1)..].Trim());
    }

    internal static string? NormalizePhPhone(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("63") && digits.Length == 12) digits = "0" + digits[2..];
        if (digits.StartsWith('9') && digits.Length == 10) digits = "0" + digits;
        return digits.Length == 11 && digits.StartsWith("09") ? digits : null;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseEmployeeType(string value, out EmployeeType result)
    {
        result = default;
        if (string.Equals(value, "Commission", StringComparison.OrdinalIgnoreCase))
        {
            result = EmployeeType.Commission;
            return true;
        }
        if (string.Equals(value, "Daily", StringComparison.OrdinalIgnoreCase))
        {
            result = EmployeeType.Daily;
            return true;
        }
        if (string.Equals(value, "Hybrid", StringComparison.OrdinalIgnoreCase))
        {
            result = EmployeeType.Hybrid;
            return true;
        }
        return false;
    }

    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd", "yyyy/MM/dd",
        "MM/dd/yyyy", "M/d/yyyy",
        "MM-dd-yyyy", "M-d-yyyy",
        "dd/MM/yyyy", "d/M/yyyy",
        "MMMM d, yyyy", "MMM d, yyyy",
    ];

    private static bool TryParseDate(string value, out DateOnly result)
    {
        result = default;
        return DateOnly.TryParseExact(value.Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
            || DateOnly.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }
}
