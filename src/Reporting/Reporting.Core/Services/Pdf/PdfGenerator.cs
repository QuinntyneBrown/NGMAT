using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Reporting.Core.Models;

namespace Reporting.Core.Services.Pdf;

/// <summary>
/// PDF generation service using PdfSharpCore (Windows-compatible)
/// </summary>
public sealed class PdfGenerator
{
    // Page dimensions (A4 in points)
    private const double PageWidth = 595.0;
    private const double PageHeight = 842.0;
    private const double Margin = 56.7; // ~2cm
    private const double ContentWidth = PageWidth - (2 * Margin);

    // Colors
    private static readonly XColor BlueMedium = XColor.FromArgb(0x21, 0x96, 0xF3);
    private static readonly XColor BlueDark = XColor.FromArgb(0x19, 0x76, 0xD2);
    private static readonly XColor GreyLight = XColor.FromArgb(0xEE, 0xEE, 0xEE);
    private static readonly XColor GreyLighter = XColor.FromArgb(0xF5, 0xF5, 0xF5);

    // Fonts
    private static readonly XFont TitleFont = new("Arial", 20, XFontStyle.Bold);
    private static readonly XFont SectionFont = new("Arial", 14, XFontStyle.Bold);
    private static readonly XFont NormalFont = new("Arial", 11, XFontStyle.Regular);
    private static readonly XFont BoldFont = new("Arial", 11, XFontStyle.Bold);
    private static readonly XFont ItalicFont = new("Arial", 11, XFontStyle.Italic);
    private static readonly XFont SmallFont = new("Arial", 9, XFontStyle.Regular);

    public byte[] GenerateMissionReportPdf(MissionReport report)
    {
        using var document = new PdfDocument();
        document.Info.Title = report.Title;
        document.Info.Author = "NGMAT Reporting Service";
        document.Info.Subject = "Mission Report";

        var page = document.AddPage();
        page.Width = PageWidth;
        page.Height = PageHeight;

        using var gfx = XGraphics.FromPdfPage(page);
        var yPosition = Margin;

        // Header - Title
        var titleRect = new XRect(0, yPosition, PageWidth, 30);
        gfx.DrawString(report.Title, TitleFont, new XSolidBrush(BlueMedium), titleRect, XStringFormats.TopCenter);
        yPosition += 50;

        // Mission Information Section
        yPosition = RenderSectionHeader(gfx, "Mission Information", yPosition);
        yPosition = RenderText(gfx, $"Mission ID: {report.MissionId}", yPosition);
        yPosition = RenderText(gfx, $"Mission Name: {report.MissionName}", yPosition);

        if (!string.IsNullOrEmpty(report.Description))
        {
            yPosition = RenderText(gfx, $"Description: {report.Description}", yPosition);
        }
        if (report.StartDate.HasValue)
        {
            yPosition = RenderText(gfx, $"Start Date: {report.StartDate:yyyy-MM-dd HH:mm:ss} UTC", yPosition);
        }
        if (report.EndDate.HasValue)
        {
            yPosition = RenderText(gfx, $"End Date: {report.EndDate:yyyy-MM-dd HH:mm:ss} UTC", yPosition);
        }
        yPosition += 15;

        // Spacecraft Section
        if (report.Spacecraft.Any())
        {
            yPosition = RenderSectionHeader(gfx, "Spacecraft", yPosition);

            // Table headers
            var spacecraftColumns = new[] { "Name", "Type", "Dry Mass (kg)", "Initial Fuel (kg)", "Current Fuel (kg)" };
            var spacecraftWidths = new[] { ContentWidth * 0.27, ContentWidth * 0.18, ContentWidth * 0.18, ContentWidth * 0.18, ContentWidth * 0.18 };
            yPosition = RenderTableHeader(gfx, spacecraftColumns, spacecraftWidths, yPosition);

            foreach (var sc in report.Spacecraft)
            {
                var values = new[] { sc.Name, sc.Type, $"{sc.DryMassKg:F2}", $"{sc.InitialFuelMassKg:F2}", $"{sc.CurrentFuelMassKg:F2}" };
                yPosition = RenderTableRow(gfx, values, spacecraftWidths, yPosition);

                // Check for page break
                if (yPosition > PageHeight - 100)
                {
                    yPosition = AddNewPage(document, gfx, out var newGfx);
                    gfx.Dispose();
                    continue;
                }
            }
            yPosition += 15;
        }

        // Maneuvers Section
        if (report.Maneuvers.Any())
        {
            yPosition = RenderSectionHeader(gfx, "Maneuvers", yPosition);

            var maneuverColumns = new[] { "Name", "Type", "Planned Epoch", "ΔV (m/s)", "Fuel (kg)", "Status" };
            var maneuverWidths = new[] { ContentWidth * 0.21, ContentWidth * 0.14, ContentWidth * 0.21, ContentWidth * 0.14, ContentWidth * 0.14, ContentWidth * 0.14 };
            yPosition = RenderTableHeader(gfx, maneuverColumns, maneuverWidths, yPosition);

            foreach (var m in report.Maneuvers)
            {
                var values = new[]
                {
                    m.Name,
                    m.Type,
                    $"{m.PlannedEpoch:yyyy-MM-dd HH:mm}",
                    $"{m.DeltaVMps:F3}",
                    $"{m.FuelUsedKg:F2}",
                    m.Status
                };
                yPosition = RenderTableRow(gfx, values, maneuverWidths, yPosition);
            }
        }

        // Footer
        RenderFooter(gfx, page, $"Generated: {report.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC | Page 1 of 1");

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    public byte[] GenerateDeltaVBudgetPdf(DeltaVBudget budget)
    {
        using var document = new PdfDocument();
        document.Info.Title = $"Delta-V Budget Report: {budget.MissionName}";
        document.Info.Author = "NGMAT Reporting Service";
        document.Info.Subject = "Delta-V Budget Report";

        var page = document.AddPage();
        page.Width = PageWidth;
        page.Height = PageHeight;

        using var gfx = XGraphics.FromPdfPage(page);
        var yPosition = Margin;

        // Header - Title
        var titleRect = new XRect(0, yPosition, PageWidth, 30);
        gfx.DrawString($"Delta-V Budget Report: {budget.MissionName}", TitleFont, new XSolidBrush(BlueMedium), titleRect, XStringFormats.TopCenter);
        yPosition += 50;

        // Fuel Summary Section
        yPosition = RenderSectionHeader(gfx, "Fuel Summary", yPosition);
        yPosition = RenderText(gfx, $"Initial Fuel: {budget.InitialFuelKg:F2} kg", yPosition);
        yPosition = RenderText(gfx, $"Remaining Fuel: {budget.RemainingFuelKg:F2} kg", yPosition);
        yPosition = RenderText(gfx, $"Fuel Used: {budget.TotalFuelUsedKg:F2} kg", yPosition);
        yPosition = RenderTextBold(gfx, $"Total Delta-V: {budget.TotalDeltaVMps:F3} m/s", yPosition);
        yPosition += 15;

        // Maneuvers Breakdown Section
        if (budget.Maneuvers.Any())
        {
            yPosition = RenderSectionHeader(gfx, "Maneuver Breakdown", yPosition);

            var columns = new[] { "#", "Name", "Epoch", "Type", "ΔV (m/s)", "Fuel (kg)", "Status" };
            var widths = new[] { ContentWidth * 0.06, ContentWidth * 0.19, ContentWidth * 0.19, ContentWidth * 0.13, ContentWidth * 0.13, ContentWidth * 0.13, ContentWidth * 0.13 };
            yPosition = RenderTableHeader(gfx, columns, widths, yPosition);

            foreach (var m in budget.Maneuvers)
            {
                var values = new[]
                {
                    $"{m.Sequence}",
                    m.Name,
                    $"{m.Epoch:MM/dd HH:mm}",
                    m.Type,
                    $"{m.DeltaVMps:F3}",
                    $"{m.FuelUsedKg:F2}",
                    m.Status
                };
                yPosition = RenderTableRow(gfx, values, widths, yPosition);
            }
        }

        // Footer
        RenderFooter(gfx, page, $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC | Page 1 of 1");

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static double RenderSectionHeader(XGraphics gfx, string title, double yPosition)
    {
        gfx.DrawString(title, SectionFont, new XSolidBrush(BlueDark), Margin, yPosition);
        yPosition += 20;

        // Draw underline
        gfx.DrawLine(new XPen(GreyLight, 1), Margin, yPosition, Margin + ContentWidth, yPosition);
        yPosition += 10;

        return yPosition;
    }

    private static double RenderText(XGraphics gfx, string text, double yPosition)
    {
        gfx.DrawString(text, NormalFont, XBrushes.Black, Margin, yPosition);
        return yPosition + 18;
    }

    private static double RenderTextBold(XGraphics gfx, string text, double yPosition)
    {
        gfx.DrawString(text, BoldFont, XBrushes.Black, Margin, yPosition);
        return yPosition + 18;
    }

    private static double RenderTableHeader(XGraphics gfx, string[] headers, double[] widths, double yPosition)
    {
        var xPos = Margin;
        var rowHeight = 22.0;

        // Draw header background
        gfx.DrawRectangle(new XSolidBrush(GreyLighter), Margin, yPosition - 5, ContentWidth, rowHeight);

        for (var i = 0; i < headers.Length; i++)
        {
            var rect = new XRect(xPos, yPosition, widths[i], rowHeight);
            gfx.DrawString(headers[i], BoldFont, XBrushes.Black, rect, XStringFormats.TopLeft);
            xPos += widths[i];
        }

        // Draw bottom border
        yPosition += rowHeight;
        gfx.DrawLine(new XPen(GreyLight, 1), Margin, yPosition, Margin + ContentWidth, yPosition);

        return yPosition + 5;
    }

    private static double RenderTableRow(XGraphics gfx, string[] values, double[] widths, double yPosition)
    {
        var xPos = Margin;
        var rowHeight = 20.0;

        for (var i = 0; i < values.Length; i++)
        {
            var rect = new XRect(xPos, yPosition, widths[i], rowHeight);
            gfx.DrawString(values[i] ?? "", NormalFont, XBrushes.Black, rect, XStringFormats.TopLeft);
            xPos += widths[i];
        }

        // Draw bottom border
        yPosition += rowHeight;
        gfx.DrawLine(new XPen(GreyLighter, 0.5), Margin, yPosition, Margin + ContentWidth, yPosition);

        return yPosition + 3;
    }

    private static void RenderFooter(XGraphics gfx, PdfPage page, string text)
    {
        var footerY = page.Height - 40;
        var rect = new XRect(0, footerY, page.Width, 20);
        gfx.DrawString(text, ItalicFont, XBrushes.Gray, rect, XStringFormats.TopCenter);
    }

    private static double AddNewPage(PdfDocument document, XGraphics currentGfx, out XGraphics newGfx)
    {
        var page = document.AddPage();
        page.Width = PageWidth;
        page.Height = PageHeight;
        newGfx = XGraphics.FromPdfPage(page);
        return Margin;
    }
}
