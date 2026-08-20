using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SetwanDokuFoto.Core.Models;
using SetwanDokuFoto.Core.Services;
using System.Collections.ObjectModel;

namespace SetwanDokuFoto.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDocxExportService _docxService;
    private readonly IPrintService _printService;
    private readonly IProjectRepository _repo;

    [ObservableProperty]
    private DocumentProject _currentProject = new();

    [ObservableProperty]
    private int _activePageIndex = 0;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "Siap membuat dokumen kolase foto resmi Setwan";

    public ObservableCollection<PhotoItem> MediaGallery { get; } = new();

    public MainViewModel(
        IDocxExportService docxService,
        IPrintService printService,
        IProjectRepository repo)
    {
        _docxService = docxService;
        _printService = printService;
        _repo = repo;

        InitializeDefaultProject();
    }

    private void InitializeDefaultProject()
    {
        CurrentProject = new DocumentProject
        {
            Title = "DOKUMENTASI FOTO KEGIATAN SEKRETARIAT DPRD KOTA BITUNG",
            PaperSize = "F4",
            Orientation = "Portrait"
        };
        
        var page1 = new DocumentPage
        {
            Title = "DOKUMENTASI SIDANG PARIPURNA DPRD KOTA BITUNG",
            Subtitle = "Masa Persidangan Ketiga Tahun Sidang 2025/2026",
            TemplateLayoutId = "grid-4-2x2"
        };

        page1.MetaTable.Add(new MetaTableItem { Label = "Hari / Tanggal", Value = "Senin, 17 Agustus 2026" });
        page1.MetaTable.Add(new MetaTableItem { Label = "Waktu Pelaksanaan", Value = "09:30 WITA - Selesai" });
        page1.MetaTable.Add(new MetaTableItem { Label = "Tempat / Lokasi", Value = "Ruang Sidang Paripurna DPRD Kota Bitung" });

        // Add 4 cells for 2x2 grid
        for (int r = 0; r < 2; r++)
        {
            for (int c = 0; c < 2; c++)
            {
                page1.Cells.Add(new CollageCell
                {
                    Row = r,
                    Column = c,
                    Caption = $"Foto {r * 2 + c + 1}: Dokumentasi Resmi Setwan"
                });
            }
        }

        CurrentProject.Pages.Add(page1);
    }

    [RelayCommand]
    private async Task ExportDocxAsync()
    {
        IsBusy = true;
        StatusMessage = "Mengekspor dokumen ke format .docx...";
        try
        {
            var path = $"Dokumentasi_Setwan_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
            await _docxService.ExportToDocxAsync(CurrentProject, path);
            StatusMessage = $"Dokumen .docx berhasil disimpan di {path}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Gagal mengekspor .docx: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PrintDocumentAsync()
    {
        IsBusy = true;
        StatusMessage = "Menghubungkan ke printer...";
        try
        {
            var printers = await _printService.GetAvailablePrintersAsync();
            var targetPrinter = printers.FirstOrDefault() ?? "Microsoft Print to PDF";
            await _printService.PrintDocumentAsync(CurrentProject, targetPrinter);
            StatusMessage = $"Dokumen dikirim ke printer: {targetPrinter}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
