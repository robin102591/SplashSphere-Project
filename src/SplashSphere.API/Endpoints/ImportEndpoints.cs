using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Import;

namespace SplashSphere.API.Endpoints;

public static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/import")
            .RequireAuthorization()
            .WithTags("Import")
            .DisableAntiforgery();

        // GET /api/v1/import/templates/{type}
        group.MapGet("/templates/{type}", GetTemplate)
            .WithName("GetImportTemplate")
            .WithSummary("Download a CSV template for the given import type.");

        // POST /api/v1/import/detect
        group.MapPost("/detect", DetectColumns)
            .WithName("DetectImportColumns")
            .WithSummary("Upload a file and detect columns + preview rows.");

        // POST /api/v1/import/validate
        group.MapPost("/validate", ValidateImport)
            .WithName("ValidateImport")
            .WithSummary("Validate a file with column mappings applied.");

        // POST /api/v1/import/execute
        group.MapPost("/execute", ExecuteImport)
            .WithName("ExecuteImport")
            .WithSummary("Execute the import after validation.");

        return app;
    }

    private static IResult GetTemplate(ImportType type, IDataMigrationService service)
    {
        var bytes = service.GenerateTemplate(type);
        var fileName = type switch
        {
            ImportType.Customers => "customers_template.csv",
            ImportType.Vehicles => "vehicles_template.csv",
            ImportType.Employees => "employees_template.csv",
            ImportType.Services => "services_template.csv",
            _ => "template.csv"
        };
        return TypedResults.File(bytes, "text/csv", fileName);
    }

    private static async Task<IResult> DetectColumns(
        HttpRequest request,
        IDataMigrationService service,
        CancellationToken ct)
    {
        var form = await request.ReadFormAsync(ct);
        var typeStr = form["type"].ToString();
        var file = form.Files.GetFile("file");

        if (file is null || file.Length == 0)
            return TypedResults.BadRequest(new { error = "File is required." });

        if (!Enum.TryParse<ImportType>(typeStr, ignoreCase: true, out var type))
            return TypedResults.BadRequest(new { error = "Invalid import type." });

        await using var stream = file.OpenReadStream();
        var result = await service.DetectColumnsAsync(type, stream, file.FileName, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> ValidateImport(
        HttpRequest request,
        IDataMigrationService service,
        CancellationToken ct)
    {
        var (type, file, mapping) = await ParseImportForm(request, ct);
        if (file is null)
            return TypedResults.BadRequest(new { error = "File is required." });
        if (mapping is null)
            return TypedResults.BadRequest(new { error = "Column mapping is required." });

        await using var stream = file.OpenReadStream();
        var result = await service.ValidateAsync(type, stream, file.FileName, mapping, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> ExecuteImport(
        HttpRequest request,
        IDataMigrationService service,
        CancellationToken ct)
    {
        var (type, file, mapping) = await ParseImportForm(request, ct);
        if (file is null)
            return TypedResults.BadRequest(new { error = "File is required." });
        if (mapping is null)
            return TypedResults.BadRequest(new { error = "Column mapping is required." });

        await using var stream = file.OpenReadStream();
        var result = await service.ExecuteAsync(type, stream, file.FileName, mapping, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<(ImportType Type, IFormFile? File, ColumnMapping? Mapping)> ParseImportForm(
        HttpRequest request, CancellationToken ct)
    {
        var form = await request.ReadFormAsync(ct);
        var typeStr = form["type"].ToString();
        var file = form.Files.GetFile("file");
        var mappingJson = form["mapping"].ToString();

        Enum.TryParse<ImportType>(typeStr, ignoreCase: true, out var type);

        ColumnMapping? mapping = null;
        if (!string.IsNullOrEmpty(mappingJson))
        {
            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(mappingJson);
            if (dict is not null)
                mapping = new ColumnMapping(dict);
        }

        return (type, file, mapping);
    }
}
