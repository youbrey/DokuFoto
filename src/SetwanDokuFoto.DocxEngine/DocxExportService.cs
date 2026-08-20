using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SetwanDokuFoto.Core.Models;
using SetwanDokuFoto.Core.Services;

namespace SetwanDokuFoto.DocxEngine;

public class DocxExportService : IDocxExportService
{
    public async Task ExportToDocxAsync(DocumentProject project, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(project);
        if (string.IsNullOrWhiteSpace(outputPath)) throw new ArgumentException("Output path is required.", nameof(outputPath));

        await Task.Run(() => Export(project, outputPath));
    }

    private static void Export(DocumentProject project, string outputPath)
    {
        using var document = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document(new Body());
        var body = mainPart.Document.Body!;
        uint drawingId = 1;

        for (var pageIndex = 0; pageIndex < project.Pages.Count; pageIndex++)
        {
            var page = project.Pages[pageIndex];
            if (pageIndex > 0) body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

            if (project.KopSurat.Enabled) AppendLetterhead(body, project.KopSurat);
            AppendCenteredText(body, page.Title, 26, bold: true);
            if (!string.IsNullOrWhiteSpace(page.Subtitle))
                AppendCenteredText(body, page.Subtitle, 20, italic: true);

            AppendMetadata(body, page.MetaTable);
            AppendPhotoGrid(mainPart, body, page, ref drawingId);
        }

        body.Append(CreateSectionProperties(project));
        mainPart.Document.Save();
    }

    private static void AppendLetterhead(Body body, KopSurat kop)
    {
        AppendCenteredText(body, kop.GovernmentName, 22, bold: true, after: 0);
        AppendCenteredText(body, kop.AgencyName, 26, bold: true, after: 0);
        if (!string.IsNullOrWhiteSpace(kop.SubAgencyName))
            AppendCenteredText(body, kop.SubAgencyName, 24, bold: true, after: 0);
        AppendCenteredText(body, kop.Address, 18, after: 0);
        AppendCenteredText(body, kop.ContactInfo, 16, italic: true, after: 20);

        body.Append(new Paragraph
        {
            ParagraphProperties = new ParagraphProperties(
                new ParagraphBorders(
                    new BottomBorder
                    {
                        Val = BorderValues.Double,
                        Size = 18,
                        Space = 3,
                        Color = "000000"
                    }),
                new SpacingBetweenLines { After = "100" })
        });
    }

    private static void AppendCenteredText(
        Body body,
        string text,
        int halfPoints,
        bool bold = false,
        bool italic = false,
        int after = 60)
    {
        var runProperties = new RunProperties(new FontSize { Val = halfPoints.ToString() });
        if (bold) runProperties.Append(new Bold());
        if (italic) runProperties.Append(new Italic());

        body.Append(new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Center },
                new SpacingBetweenLines { After = after.ToString() }),
            new Run(runProperties, new Text(text))));
    }

    private static void AppendMetadata(Body body, IEnumerable<MetaTableItem> items)
    {
        var metadata = items.Where(item =>
            !string.IsNullOrWhiteSpace(item.Label) || !string.IsNullOrWhiteSpace(item.Value)).ToList();
        if (metadata.Count == 0) return;

        var table = new Table(new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            new TableBorders(
                new TopBorder { Val = BorderValues.Nil },
                new LeftBorder { Val = BorderValues.Nil },
                new BottomBorder { Val = BorderValues.Nil },
                new RightBorder { Val = BorderValues.Nil },
                new InsideHorizontalBorder { Val = BorderValues.Nil },
                new InsideVerticalBorder { Val = BorderValues.Nil })));

        foreach (var item in metadata)
        {
            table.Append(new TableRow(
                CreateTextCell(item.Label, bold: true, widthPercent: "1500"),
                CreateTextCell($": {item.Value}", widthPercent: "3500")));
        }

        body.Append(table, new Paragraph(new ParagraphProperties(new SpacingBetweenLines { After = "60" })));
    }

    private static TableCell CreateTextCell(string text, bool bold = false, string widthPercent = "2500")
    {
        var properties = new RunProperties(new FontSize { Val = "18" });
        if (bold) properties.Append(new Bold());
        return new TableCell(
            new TableCellProperties(
                new TableCellWidth { Type = TableWidthUnitValues.Pct, Width = widthPercent },
                new TableCellMargin(
                    new TopMargin { Width = "20", Type = TableWidthUnitValues.Dxa },
                    new BottomMargin { Width = "20", Type = TableWidthUnitValues.Dxa })),
            new Paragraph(new Run(properties, new Text(text))));
    }

    private static void AppendPhotoGrid(
        MainDocumentPart mainPart,
        Body body,
        DocumentPage page,
        ref uint drawingId)
    {
        if (page.Cells.Count == 0) return;

        var columnCount = Math.Max(1, page.Cells.Max(cell => cell.Column + cell.ColumnSpan));
        var cellWidthPercent = (5000 / columnCount).ToString();
        var drawingWidthEmu = 6_000_000L / columnCount;
        var table = new Table(new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            new TableJustification { Val = TableRowAlignmentValues.Center },
            new TableLayout { Type = TableLayoutValues.Fixed },
            new TableBorders(
                new TopBorder { Val = BorderValues.Nil },
                new LeftBorder { Val = BorderValues.Nil },
                new BottomBorder { Val = BorderValues.Nil },
                new RightBorder { Val = BorderValues.Nil },
                new InsideHorizontalBorder { Val = BorderValues.Nil },
                new InsideVerticalBorder { Val = BorderValues.Nil })));

        foreach (var rowGroup in page.Cells.GroupBy(cell => cell.Row).OrderBy(group => group.Key))
        {
            var row = new TableRow();
            foreach (var cell in rowGroup.OrderBy(item => item.Column))
            {
                var tableCell = new TableCell(new TableCellProperties(
                    new TableCellWidth { Type = TableWidthUnitValues.Pct, Width = cellWidthPercent },
                    new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center },
                    new TableCellMargin(
                        new TopMargin { Width = "55", Type = TableWidthUnitValues.Dxa },
                        new LeftMargin { Width = "55", Type = TableWidthUnitValues.Dxa },
                        new BottomMargin { Width = "55", Type = TableWidthUnitValues.Dxa },
                        new RightMargin { Width = "55", Type = TableWidthUnitValues.Dxa })));

                var imageParagraph = new Paragraph(new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center },
                    new SpacingBetweenLines { After = "30" }));

                if (HasSource(cell.Photo))
                {
                    var (aspectWidth, aspectHeight) = CanvaCropMath.ParseAspectRatio(cell.AspectRatio);
                    const int renderWidth = 1600;
                    var renderHeight = Math.Max(300, (int)Math.Round(renderWidth * aspectHeight / aspectWidth));
                    var jpeg = CropImageRenderer.RenderJpeg(cell, renderWidth, renderHeight);
                    var imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);
                    using (var stream = new MemoryStream(jpeg)) imagePart.FeedData(stream);

                    var relationshipId = mainPart.GetIdOfPart(imagePart);
                    var drawingHeightEmu = (long)Math.Round(drawingWidthEmu * renderHeight / (double)renderWidth);
                    imageParagraph.Append(new Run(CreateImageDrawing(
                        relationshipId,
                        cell.Photo!.FileName,
                        drawingWidthEmu,
                        drawingHeightEmu,
                        drawingId++)));
                }
                else
                {
                    imageParagraph.Append(new Run(
                        new RunProperties(new Color { Val = "94A3B8" }, new FontSize { Val = "18" }),
                        new Text("[ Frame foto kosong ]")));
                }

                tableCell.Append(imageParagraph);
                if (cell.ShowCaption && !string.IsNullOrWhiteSpace(cell.Caption))
                {
                    tableCell.Append(new Paragraph(
                        new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                        new Run(
                            new RunProperties(new Bold(), new FontSize { Val = "17" }),
                            new Text(cell.Caption))));
                }

                row.Append(tableCell);
            }

            table.Append(row);
        }

        body.Append(table);
    }

    private static bool HasSource(PhotoItem? photo) =>
        photo is not null &&
        (photo.ImageBytes is { Length: > 0 } ||
         (!string.IsNullOrWhiteSpace(photo.FilePath) && File.Exists(photo.FilePath)));

    private static Drawing CreateImageDrawing(
        string relationshipId,
        string? fileName,
        long widthEmu,
        long heightEmu,
        uint drawingId)
    {
        var name = string.IsNullOrWhiteSpace(fileName) ? $"Photo {drawingId}" : fileName;
        return new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = widthEmu, Cy = heightEmu },
                new DW.EffectExtent { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 },
                new DW.DocProperties { Id = drawingId, Name = name },
                new DW.NonVisualGraphicFrameDrawingProperties(
                    new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties { Id = 0U, Name = name },
                                new PIC.NonVisualPictureDrawingProperties()),
                            new PIC.BlipFill(
                                new A.Blip { Embed = relationshipId, CompressionState = A.BlipCompressionValues.Print },
                                new A.Stretch(new A.FillRectangle())),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset { X = 0, Y = 0 },
                                    new A.Extents { Cx = widthEmu, Cy = heightEmu }),
                                new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle })))
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }))
            {
                DistanceFromTop = 0U,
                DistanceFromBottom = 0U,
                DistanceFromLeft = 0U,
                DistanceFromRight = 0U
            });
    }

    private static SectionProperties CreateSectionProperties(DocumentProject project)
    {
        var isLandscape = project.Orientation.Equals("Landscape", StringComparison.OrdinalIgnoreCase);
        var (portraitWidth, portraitHeight) = project.PaperSize.ToUpperInvariant() switch
        {
            "F4" => (12189U, 18709U),
            "LETTER" => (12240U, 15840U),
            "LEGAL" => (12240U, 20160U),
            _ => (11906U, 16838U)
        };

        return new SectionProperties(
            new PageSize
            {
                Width = isLandscape ? portraitHeight : portraitWidth,
                Height = isLandscape ? portraitWidth : portraitHeight,
                Orient = isLandscape ? PageOrientationValues.Landscape : PageOrientationValues.Portrait
            },
            new PageMargin
            {
                Top = (int)Math.Round(project.Margins.Top * 567),
                Bottom = (int)Math.Round(project.Margins.Bottom * 567),
                Left = (int)Math.Round(project.Margins.Left * 567),
                Right = (int)Math.Round(project.Margins.Right * 567)
            });
    }
}
