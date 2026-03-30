using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SplashSphere.Application.Features.Payroll.Queries.ExportPayslipPdf;

public sealed class PayslipPdfDocument(PayslipDto data) : IDocument
{
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A5);
            page.MarginHorizontal(30);
            page.MarginVertical(25);
            page.DefaultTextStyle(x => x.FontSize(9));

            page.Header().Column(col =>
            {
                col.Item().AlignCenter().Text(data.TenantName).Bold().FontSize(14);
                col.Item().AlignCenter().Text(data.BranchName).FontSize(10);
                col.Item().PaddingTop(8).AlignCenter().Text("PAYSLIP").Bold().FontSize(12);
                col.Item().AlignCenter().Text($"{data.PeriodLabel}  |  {data.PeriodStart:MMM d} – {data.PeriodEnd:MMM d, yyyy}")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
                col.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
            });

            page.Content().PaddingTop(10).Column(col =>
            {
                // Employee info
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Employee: {data.EmployeeName}").Bold();
                    row.RelativeItem().AlignRight().Text($"Type: {data.EmployeeType}");
                });

                if (data.EmployeeId is not null)
                    col.Item().Text($"ID: {data.EmployeeId}").FontSize(8).FontColor(Colors.Grey.Medium);

                col.Item().PaddingTop(8).Text($"Days Worked: {data.DaysWorked}  |  Commission Transactions: {data.CommissionTransactions}")
                    .FontSize(8);

                // Earnings
                col.Item().PaddingTop(10).Text("EARNINGS").Bold().FontSize(8).FontColor(Colors.Blue.Medium);
                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(1);
                    });

                    AddRow(table, "Base Salary", data.BaseSalary);
                    AddRow(table, "Commissions", data.TotalCommissions);
                    if (data.TotalTips > 0)
                        AddRow(table, "Tips (paid out)", data.TotalTips, italic: true);
                    AddRow(table, "Gross Earnings", data.GrossEarnings, bold: true);
                });

                // Bonuses
                if (data.Bonuses.Count > 0)
                {
                    col.Item().PaddingTop(10).Text("BONUSES").Bold().FontSize(8).FontColor(Colors.Green.Medium);
                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                        });

                        foreach (var b in data.Bonuses)
                            AddRow(table, b.Category + (b.Notes is not null ? $" — {b.Notes}" : ""), b.Amount);
                        AddRow(table, "Total Bonuses", data.TotalBonuses, bold: true);
                    });
                }

                // Deductions
                if (data.Deductions.Count > 0)
                {
                    col.Item().PaddingTop(10).Text("DEDUCTIONS").Bold().FontSize(8).FontColor(Colors.Red.Medium);
                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                        });

                        foreach (var d in data.Deductions)
                            AddRow(table, d.Category + (d.Notes is not null ? $" — {d.Notes}" : ""), d.Amount);
                        AddRow(table, "Total Deductions", data.TotalDeductions, bold: true);
                    });
                }

                // Net Pay
                col.Item().PaddingTop(12).LineHorizontal(1f).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Text("NET PAY").Bold().FontSize(11);
                    row.RelativeItem().AlignRight().Text(Peso(data.NetPay)).Bold().FontSize(11);
                });
            });

            page.Footer().AlignCenter().Text($"Generated {data.GeneratedAt:MMM d, yyyy h:mm tt}")
                .FontSize(7).FontColor(Colors.Grey.Medium);
        });
    }

    private static void AddRow(TableDescriptor table, string label, decimal amount,
        bool bold = false, bool italic = false)
    {
        table.Cell().PaddingVertical(1).Text(text =>
        {
            var span = text.Span(label);
            if (bold) span.Bold();
            if (italic) span.Italic();
        });
        table.Cell().PaddingVertical(1).AlignRight().Text(text =>
        {
            var span = text.Span(Peso(amount));
            if (bold) span.Bold();
        });
    }

    private static string Peso(decimal amount) => $"₱{amount:N2}";
}
