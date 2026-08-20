using SetwanDokuFoto.Core.Models;
using SetwanDokuFoto.Core.Services;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SetwanDokuFoto.PrintEngine;

public class WpfPrintService : IPrintService
{
    public Task<List<string>> GetAvailablePrintersAsync()
    {
        var printers = new List<string>();
        try
        {
            using var printServer = new LocalPrintServer();
            foreach (var queue in printServer.GetPrintQueues(new[]
                     {
                         EnumeratedPrintQueueTypes.Local,
                         EnumeratedPrintQueueTypes.Connections
                     }))
            {
                printers.Add(queue.FullName);
            }
        }
        catch
        {
            // The print dialog can still resolve the Windows default printer.
        }

        if (printers.Count == 0) printers.Add("Windows Default Printer");

        return Task.FromResult(printers);
    }

    public Task PrintDocumentAsync(DocumentProject project, string printerName)
    {
        ArgumentNullException.ThrowIfNull(project);

        void PrintOnUiThread()
        {
            var dialog = new PrintDialog();
            try
            {
                if (!string.IsNullOrWhiteSpace(printerName))
                {
                    using var server = new LocalPrintServer();
                    dialog.PrintQueue = server.GetPrintQueue(printerName);
                }
            }
            catch
            {
                // Keep the Windows default printer selected.
            }

            if (dialog.ShowDialog() != true) return;
            var document = BuildPrintDocument(project, dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
            dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, project.Title);
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess()) dispatcher.Invoke(PrintOnUiThread);
        else PrintOnUiThread();

        return Task.CompletedTask;
    }

    private static FlowDocument BuildPrintDocument(DocumentProject project, double printableWidth, double printableHeight)
    {
        var document = new FlowDocument
        {
            FontFamily = new FontFamily(project.FontFamily),
            FontSize = 10,
            PageWidth = printableWidth,
            PageHeight = printableHeight,
            PagePadding = new Thickness(
                CmToDip(project.Margins.Left),
                CmToDip(project.Margins.Top),
                CmToDip(project.Margins.Right),
                CmToDip(project.Margins.Bottom)),
            ColumnWidth = double.PositiveInfinity
        };

        for (var pageIndex = 0; pageIndex < project.Pages.Count; pageIndex++)
        {
            if (pageIndex > 0) document.Blocks.Add(new Paragraph(new Run(string.Empty)) { BreakPageBefore = true });
            var page = project.Pages[pageIndex];

            if (project.KopSurat.Enabled)
            {
                document.Blocks.Add(Centered(project.KopSurat.GovernmentName, 12, FontWeights.Bold));
                document.Blocks.Add(Centered(project.KopSurat.AgencyName, 14, FontWeights.Black));
                document.Blocks.Add(Centered(project.KopSurat.SubAgencyName, 12, FontWeights.Bold));
                document.Blocks.Add(Centered(project.KopSurat.Address, 8, FontWeights.Normal));
            }

            document.Blocks.Add(Centered(page.Title, 13, FontWeights.Black, new Thickness(0, 12, 0, 1)));
            if (!string.IsNullOrWhiteSpace(page.Subtitle))
                document.Blocks.Add(Centered(page.Subtitle, 10, FontWeights.Normal, new Thickness(0, 0, 0, 8)));

            var columnCount = Math.Max(1, page.Cells.Select(cell => cell.Column).DefaultIfEmpty(0).Max() + 1);
            var table = new Table { CellSpacing = 7 };
            for (var column = 0; column < columnCount; column++) table.Columns.Add(new TableColumn());

            foreach (var rowGroup in page.Cells.GroupBy(cell => cell.Row).OrderBy(group => group.Key))
            {
                var row = new TableRow();
                foreach (var cell in rowGroup.OrderBy(item => item.Column))
                {
                    var block = new TableCell { Padding = new Thickness(2) };
                    if (TryRenderPhoto(cell, out var source))
                    {
                        var (ratioWidth, ratioHeight) = CanvaCropMath.ParseAspectRatio(cell.AspectRatio);
                        block.Blocks.Add(new BlockUIContainer(new Image
                        {
                            Source = source,
                            Stretch = Stretch.Uniform,
                            Width = Math.Max(120, (printableWidth - 100) / columnCount),
                            Height = Math.Max(90, (printableWidth - 100) / columnCount * ratioHeight / ratioWidth)
                        }));
                    }

                    if (cell.ShowCaption && !string.IsNullOrWhiteSpace(cell.Caption))
                    {
                        block.Blocks.Add(new Paragraph(new Run(cell.Caption))
                        {
                            TextAlignment = TextAlignment.Center,
                            FontWeight = FontWeights.SemiBold,
                            FontSize = 8,
                            Margin = new Thickness(0, 3, 0, 0)
                        });
                    }

                    row.Cells.Add(block);
                }

                var group = new TableRowGroup();
                group.Rows.Add(row);
                table.RowGroups.Add(group);
            }

            document.Blocks.Add(table);
        }

        return document;
    }

    private static Paragraph Centered(
        string? text,
        double fontSize,
        FontWeight weight,
        Thickness? margin = null) => new(new Run(text ?? string.Empty))
    {
        TextAlignment = TextAlignment.Center,
        FontSize = fontSize,
        FontWeight = weight,
        Margin = margin ?? new Thickness(0)
    };

    private static bool TryRenderPhoto(CollageCell cell, out BitmapSource? result)
    {
        result = null;
        var photo = cell.Photo;
        if (photo is null) return false;

        try
        {
            BitmapSource source;
            if (photo.ImageBytes is { Length: > 0 } bytes)
            {
                using var stream = new MemoryStream(bytes);
                source = LoadBitmap(stream);
            }
            else if (File.Exists(photo.FilePath))
            {
                using var stream = File.OpenRead(photo.FilePath);
                source = LoadBitmap(stream);
            }
            else
            {
                return false;
            }

            var (ratioWidth, ratioHeight) = CanvaCropMath.ParseAspectRatio(cell.AspectRatio);
            const int outputWidth = 1200;
            var outputHeight = Math.Max(300, (int)Math.Round(outputWidth * ratioHeight / ratioWidth));
            var transform = photo.Transform;
            var cover = CanvaCropMath.CalculateCoverScale(
                outputWidth, outputHeight, source.PixelWidth, source.PixelHeight);
            var coveredWidth = source.PixelWidth * cover;
            var coveredHeight = source.PixelHeight * cover;
            var rotationCover = CanvaCropMath.CalculateRotationCoverMultiplier(
                outputWidth, outputHeight, coveredWidth, coveredHeight, transform.Rotation);
            var effectiveScale = Math.Max(transform.Scale, rotationCover);
            var displayWidth = coveredWidth * effectiveScale;
            var displayHeight = coveredHeight * effectiveScale;

            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.PushClip(new RectangleGeometry(new Rect(0, 0, outputWidth, outputHeight)));
                context.PushTransform(new TranslateTransform(
                    outputWidth / 2d + transform.OffsetX * outputWidth,
                    outputHeight / 2d + transform.OffsetY * outputHeight));
                context.PushTransform(new RotateTransform(transform.Rotation));
                context.PushTransform(new ScaleTransform(
                    transform.FlipHorizontal ? -1 : 1,
                    transform.FlipVertical ? -1 : 1));
                context.DrawImage(source, new Rect(-displayWidth / 2, -displayHeight / 2, displayWidth, displayHeight));
                context.Pop();
                context.Pop();
                context.Pop();
                context.Pop();
            }

            var rendered = new RenderTargetBitmap(outputWidth, outputHeight, 150, 150, PixelFormats.Pbgra32);
            rendered.Render(visual);
            rendered.Freeze();
            result = rendered;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static BitmapSource LoadBitmap(Stream stream)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private static double CmToDip(double centimeters) => centimeters / 2.54 * 96;
}
