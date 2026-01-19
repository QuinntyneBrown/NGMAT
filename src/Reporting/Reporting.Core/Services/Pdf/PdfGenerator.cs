using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reporting.Core.Models;

namespace Reporting.Core.Services.Pdf;

/// <summary>
/// PDF generation service using QuestPDF
/// </summary>
public sealed class PdfGenerator
{
    static PdfGenerator()
    {
        // Configure QuestPDF license (Community license is free for open-source projects)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateMissionReportPdf(MissionReport report)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .AlignCenter()
                    .Text(report.Title)
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        // Mission Information
                        column.Item().Element(c => RenderSection(c, "Mission Information", container =>
                        {
                            container.Row(row =>
                            {
                                row.RelativeItem().Text($"Mission ID: {report.MissionId}");
                            });
                            container.Row(row =>
                            {
                                row.RelativeItem().Text($"Mission Name: {report.MissionName}");
                            });
                            if (!string.IsNullOrEmpty(report.Description))
                            {
                                container.Row(row =>
                                {
                                    row.RelativeItem().Text($"Description: {report.Description}");
                                });
                            }
                            if (report.StartDate.HasValue)
                            {
                                container.Row(row =>
                                {
                                    row.RelativeItem().Text($"Start Date: {report.StartDate:yyyy-MM-dd HH:mm:ss} UTC");
                                });
                            }
                            if (report.EndDate.HasValue)
                            {
                                container.Row(row =>
                                {
                                    row.RelativeItem().Text($"End Date: {report.EndDate:yyyy-MM-dd HH:mm:ss} UTC");
                                });
                            }
                        }));

                        // Spacecraft
                        if (report.Spacecraft.Any())
                        {
                            column.Item().Element(c => RenderSection(c, "Spacecraft", container =>
                            {
                                container.Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Name").Bold();
                                        header.Cell().Element(CellStyle).Text("Type").Bold();
                                        header.Cell().Element(CellStyle).Text("Dry Mass (kg)").Bold();
                                        header.Cell().Element(CellStyle).Text("Initial Fuel (kg)").Bold();
                                        header.Cell().Element(CellStyle).Text("Current Fuel (kg)").Bold();
                                    });

                                    foreach (var sc in report.Spacecraft)
                                    {
                                        table.Cell().Element(CellStyle).Text(sc.Name);
                                        table.Cell().Element(CellStyle).Text(sc.Type);
                                        table.Cell().Element(CellStyle).Text($"{sc.DryMassKg:F2}");
                                        table.Cell().Element(CellStyle).Text($"{sc.InitialFuelMassKg:F2}");
                                        table.Cell().Element(CellStyle).Text($"{sc.CurrentFuelMassKg:F2}");
                                    }
                                });
                            }));
                        }

                        // Maneuvers
                        if (report.Maneuvers.Any())
                        {
                            column.Item().Element(c => RenderSection(c, "Maneuvers", container =>
                            {
                                container.Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Name").Bold();
                                        header.Cell().Element(CellStyle).Text("Type").Bold();
                                        header.Cell().Element(CellStyle).Text("Planned Epoch").Bold();
                                        header.Cell().Element(CellStyle).Text("ΔV (m/s)").Bold();
                                        header.Cell().Element(CellStyle).Text("Fuel (kg)").Bold();
                                        header.Cell().Element(CellStyle).Text("Status").Bold();
                                    });

                                    foreach (var m in report.Maneuvers)
                                    {
                                        table.Cell().Element(CellStyle).Text(m.Name);
                                        table.Cell().Element(CellStyle).Text(m.Type);
                                        table.Cell().Element(CellStyle).Text($"{m.PlannedEpoch:yyyy-MM-dd HH:mm}");
                                        table.Cell().Element(CellStyle).Text($"{m.DeltaVMps:F3}");
                                        table.Cell().Element(CellStyle).Text($"{m.FuelUsedKg:F2}");
                                        table.Cell().Element(CellStyle).Text(m.Status);
                                    }
                                });
                            }));
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated: ");
                        x.Span($"{report.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC").Italic();
                        x.Span(" | Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateDeltaVBudgetPdf(DeltaVBudget budget)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .AlignCenter()
                    .Text($"Delta-V Budget Report: {budget.MissionName}")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        // Summary
                        column.Item().Element(c => RenderSection(c, "Fuel Summary", container =>
                        {
                            container.Row(row =>
                            {
                                row.RelativeItem().Text($"Initial Fuel: {budget.InitialFuelKg:F2} kg");
                            });
                            container.Row(row =>
                            {
                                row.RelativeItem().Text($"Remaining Fuel: {budget.RemainingFuelKg:F2} kg");
                            });
                            container.Row(row =>
                            {
                                row.RelativeItem().Text($"Fuel Used: {budget.TotalFuelUsedKg:F2} kg");
                            });
                            container.Row(row =>
                            {
                                row.RelativeItem().Text($"Total Delta-V: {budget.TotalDeltaV:F3} m/s").Bold();
                            });
                        }));

                        // Maneuvers
                        if (budget.Maneuvers.Any())
                        {
                            column.Item().Element(c => RenderSection(c, "Maneuver Breakdown", container =>
                            {
                                container.Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(40);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("#").Bold();
                                        header.Cell().Element(CellStyle).Text("Name").Bold();
                                        header.Cell().Element(CellStyle).Text("Epoch").Bold();
                                        header.Cell().Element(CellStyle).Text("Type").Bold();
                                        header.Cell().Element(CellStyle).Text("ΔV (m/s)").Bold();
                                        header.Cell().Element(CellStyle).Text("Fuel (kg)").Bold();
                                        header.Cell().Element(CellStyle).Text("Status").Bold();
                                    });

                                    foreach (var m in budget.Maneuvers)
                                    {
                                        table.Cell().Element(CellStyle).Text($"{m.Sequence}");
                                        table.Cell().Element(CellStyle).Text(m.Name);
                                        table.Cell().Element(CellStyle).Text($"{m.Epoch:MM/dd HH:mm}");
                                        table.Cell().Element(CellStyle).Text(m.Type);
                                        table.Cell().Element(CellStyle).Text($"{m.DeltaVMps:F3}");
                                        table.Cell().Element(CellStyle).Text($"{m.FuelUsedKg:F2}");
                                        table.Cell().Element(CellStyle).Text(m.Status);
                                    }
                                });
                            }));
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated: ");
                        x.Span($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC").Italic();
                        x.Span(" | Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        }).GeneratePdf();
    }

    private static void RenderSection(IContainer container, string title, Action<IContainer> content)
    {
        container.Column(column =>
        {
            column.Item().PaddingBottom(5).Text(title).Bold().FontSize(14).FontColor(Colors.Blue.Darken1);
            column.Item().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
            column.Item().PaddingTop(10).Column(content);
        });
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5);
    }
}
