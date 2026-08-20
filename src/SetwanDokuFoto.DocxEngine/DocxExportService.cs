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
        await Task.Run(() =>
        {
            using var doc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // 1. Page Properties & Margins
            var sectionProps = new SectionProperties();
            var pageSize = new PageSize()
            {
                Width = (uint)(project.PaperSize == "F4" ? 12189 : 11906), // F4 or A4 Twips
                Height = (uint)(project.PaperSize == "F4" ? 18709 : 16838),
                Orient = project.Orientation == "Landscape" ? PageOrientationValues.Landscape : PageOrientationValues.Portrait
            };
            var pageMargin = new PageMargin()
            {
                Top = (int)(project.Margins.Top * 567), // cm to twip
                Bottom = (int)(project.Margins.Bottom * 567),
                Left = (int)(project.Margins.Left * 567),
                Right = (int)(project.Margins.Right * 567)
            };
            sectionProps.Append(pageSize);
            sectionProps.Append(pageMargin);

            // 2. Kop Surat (Official Setwan Header)
            if (project.KopSurat.Enabled)
            {
                var kopPara = body.AppendChild(new Paragraph());
                kopPara.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                
                var rGov = kopPara.AppendChild(new Run(new Text(project.KopSurat.GovernmentName)));
                rGov.RunProperties = new RunProperties(new Bold(), new FontSize() { Val = "22" });

                var rAgency = body.AppendChild(new Paragraph(new Run(new Text(project.KopSurat.AgencyName))));
                rAgency.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                rAgency.GetFirstChild<Run>()!.RunProperties = new RunProperties(new Bold(), new FontSize() { Val = "26" });

                if (!string.IsNullOrEmpty(project.KopSurat.SubAgencyName))
                {
                    var rSub = body.AppendChild(new Paragraph(new Run(new Text(project.KopSurat.SubAgencyName))));
                    rSub.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                    rSub.GetFirstChild<Run>()!.RunProperties = new RunProperties(new Bold(), new FontSize() { Val = "24" });
                }

                var rAddr = body.AppendChild(new Paragraph(new Run(new Text(project.KopSurat.Address))));
                rAddr.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                rAddr.GetFirstChild<Run>()!.RunProperties = new RunProperties(new FontSize() { Val = "18" });

                var rContact = body.AppendChild(new Paragraph(new Run(new Text(project.KopSurat.ContactInfo))));
                rContact.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                rContact.GetFirstChild<Run>()!.RunProperties = new RunProperties(new Italic(), new FontSize() { Val = "16" });

                // Double Border Divider
                var linePara = body.AppendChild(new Paragraph());
                linePara.ParagraphProperties = new ParagraphProperties(
                    new ParagraphBorders(new BottomBorder() { Val = BorderValues.Double, Size = 24, Space = 4, Color = "000000" })
                );
            }

            // 3. Document Content (Pages)
            foreach (var page in project.Pages)
            {
                // Judul
                var titlePara = body.AppendChild(new Paragraph(new Run(new Text(page.Title))));
                titlePara.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                titlePara.GetFirstChild<Run>()!.RunProperties = new RunProperties(new Bold(), new FontSize() { Val = "26" });

                if (!string.IsNullOrEmpty(page.Subtitle))
                {
                    var subPara = body.AppendChild(new Paragraph(new Run(new Text(page.Subtitle))));
                    subPara.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                    subPara.GetFirstChild<Run>()!.RunProperties = new RunProperties(new Italic(), new FontSize() { Val = "20" });
                }

                // Photo Grid Table
                var table = body.AppendChild(new Table());
                var tblPr = new TableProperties(
                    new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct },
                    new TableJustification() { Val = TableRowAlignmentValues.Center }
                );
                table.AppendChild(tblPr);

                var rows = page.Cells.GroupBy(c => c.Row).OrderBy(g => g.Key);
                foreach (var r in rows)
                {
                    var tableRow = table.AppendChild(new TableRow());
                    foreach (var cell in r.OrderBy(c => c.Column))
                    {
                        var tableCell = tableRow.AppendChild(new TableCell());
                        tableCell.AppendChild(new TableCellProperties(new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = "2500" }));

                        var cellPara = tableCell.AppendChild(new Paragraph());
                        cellPara.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });

                        if (cell.Photo != null && !string.IsNullOrEmpty(cell.Photo.FilePath) && File.Exists(cell.Photo.FilePath))
                        {
                            var imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);
                            using (var stream = new FileStream(cell.Photo.FilePath, FileMode.Open, FileAccess.Read))
                            {
                                imagePart.FeedData(stream);
                            }
                        }

                        if (!string.IsNullOrEmpty(cell.Caption))
                        {
                            var capPara = tableCell.AppendChild(new Paragraph(new Run(new Text(cell.Caption))));
                            capPara.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                            capPara.GetFirstChild<Run>()!.RunProperties = new RunProperties(new Bold(), new FontSize() { Val = "18" });
                        }
                    }
                }
            }

            body.AppendChild(sectionProps);
            mainPart.Document.Save();
        });
    }
}
