using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SetwanDokuFoto.Core.Models;
using SetwanDokuFoto.Core.Services;
using System.Collections.ObjectModel;
using System.IO;

namespace SetwanDokuFoto.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDocxExportService _docxService;
    private readonly IPrintService _printService;
    private readonly IProjectRepository _repository;

    [ObservableProperty] private DocumentProject _currentProject = new();
    [ObservableProperty] private DocumentPage _activePage = new();
    [ObservableProperty] private CollageCell? _selectedCell;
    [ObservableProperty] private int _activePageIndex;
    [ObservableProperty] private int _selectedSidebarIndex;
    [ObservableProperty] private int _gridRows = 2;
    [ObservableProperty] private int _gridColumns = 2;
    [ObservableProperty] private double _gridHeight = 500;
    [ObservableProperty] private double _canvasZoom = .78;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isCropActive;
    [ObservableProperty] private string _statusMessage = "Siap membuat dokumen kolase foto resmi Setwan";

    public ObservableCollection<PhotoItem> MediaGallery { get; } = new();

    public MainViewModel(
        IDocxExportService docxService,
        IPrintService printService,
        IProjectRepository repository)
    {
        _docxService = docxService;
        _printService = printService;
        _repository = repository;
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

        var page = new DocumentPage
        {
            Title = "DOKUMENTASI SIDANG PARIPURNA DPRD KOTA BITUNG",
            Subtitle = "Masa Persidangan Ketiga Tahun Sidang 2025/2026",
            TemplateLayoutId = "grid-4-2x2"
        };

        page.MetaTable.Add(new MetaTableItem { Label = "Hari / Tanggal", Value = "Senin, 17 Agustus 2026" });
        page.MetaTable.Add(new MetaTableItem { Label = "Waktu Pelaksanaan", Value = "09:30 WITA - Selesai" });
        page.MetaTable.Add(new MetaTableItem { Label = "Tempat / Lokasi", Value = "Ruang Sidang Paripurna DPRD Kota Bitung" });

        CurrentProject.Pages.Add(page);
        ActivePage = page;
        ApplyTemplate("grid-4-2x2");
    }

    [RelayCommand]
    private void NewProject()
    {
        MediaGallery.Clear();
        SelectedCell = null;
        ActivePageIndex = 0;
        GridRows = 2;
        GridColumns = 2;
        InitializeDefaultProject();
        StatusMessage = "Proyek baru dibuat";
    }

    [RelayCommand]
    private async Task ImportPhotosAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Impor foto kegiatan",
            Filter = "File gambar|*.jpg;*.jpeg;*.png;*.bmp|Semua file|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog() != true) return;

        foreach (var path in dialog.FileNames)
        {
            var item = CreatePhoto(path);
            MediaGallery.Add(item);
            var emptyCell = ActivePage.Cells.FirstOrDefault(cell => cell.Photo is null);
            if (emptyCell is not null) emptyCell.Photo = item;
        }

        StatusMessage = $"{dialog.FileNames.Length} foto berhasil diimpor";
        await Task.CompletedTask;
    }

    public async Task ImportPhotoForCellAsync(CollageCell cell)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Pilih foto untuk frame",
            Filter = "File gambar|*.jpg;*.jpeg;*.png;*.bmp|Semua file|*.*"
        };

        if (dialog.ShowDialog() != true) return;

        var item = CreatePhoto(dialog.FileName);
        MediaGallery.Add(item);
        cell.Photo = item;
        SelectedCell = cell;
        StatusMessage = $"{item.FileName} ditempatkan ke frame";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void AssignMedia(PhotoItem? photo)
    {
        if (photo is null || SelectedCell is null)
        {
            StatusMessage = "Pilih frame terlebih dahulu sebelum menempatkan foto";
            return;
        }

        SelectedCell.Photo = photo;
        StatusMessage = $"{photo.FileName} ditempatkan ke frame terpilih";
    }

    [RelayCommand]
    private void ApplyTemplate(string? templateId)
    {
        (GridRows, GridColumns, GridHeight) = templateId switch
        {
            "hero-1" => (1, 1, 369d),
            "split-2" => (1, 2, 245d),
            "grid-6-2x3" => (3, 2, 740d),
            _ => (2, 2, 500d)
        };

        var existing = ActivePage.Cells
            .Where(cell => cell.Photo is not null)
            .Select(cell => (cell.Photo, cell.Caption))
            .ToList();

        ActivePage.Cells.Clear();
        var count = GridRows * GridColumns;
        for (var index = 0; index < count; index++)
        {
            (PhotoItem? Photo, string? Caption) previous =
                index < existing.Count ? existing[index] : (null, null);
            ActivePage.Cells.Add(new CollageCell
            {
                Row = index / GridColumns,
                Column = index % GridColumns,
                Photo = previous.Photo,
                Caption = previous.Caption ?? $"Foto {index + 1}: Dokumentasi kegiatan",
                ShowCaption = true,
                AspectRatio = GridRows == 1 && GridColumns == 1 ? "16:9" : "4:3"
            });
        }

        ActivePage.TemplateLayoutId = templateId ?? "grid-4-2x2";
        SelectedCell = null;
        StatusMessage = $"Template {GridColumns} kolom × {GridRows} baris diterapkan";
    }

    [RelayCommand]
    private void AddPage()
    {
        var page = new DocumentPage
        {
            PageNumber = CurrentProject.Pages.Count + 1,
            Title = $"DOKUMENTASI FOTO KEGIATAN (HALAMAN {CurrentProject.Pages.Count + 1})",
            Subtitle = "Sekretariat DPRD Kota Bitung"
        };

        CurrentProject.Pages.Add(page);
        ActivePageIndex = CurrentProject.Pages.Count - 1;
        ActivePage = page;
        ApplyTemplate("grid-4-2x2");
        StatusMessage = $"Halaman {page.PageNumber} ditambahkan";
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (ActivePageIndex <= 0) return;
        ActivePageIndex--;
        ActivePage = CurrentProject.Pages[ActivePageIndex];
        SyncGridFromPage();
    }

    [RelayCommand]
    private void NextPage()
    {
        if (ActivePageIndex >= CurrentProject.Pages.Count - 1) return;
        ActivePageIndex++;
        ActivePage = CurrentProject.Pages[ActivePageIndex];
        SyncGridFromPage();
    }

    [RelayCommand]
    private async Task SaveProjectAsync()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Simpan proyek DokuFoto",
            Filter = "Proyek DokuFoto|*.dokufoto.json|JSON|*.json",
            FileName = "Dokumentasi_Setwan.dokufoto.json"
        };

        if (dialog.ShowDialog() != true) return;
        await _repository.SaveProjectAsync(CurrentProject, dialog.FileName);
        StatusMessage = $"Proyek tersimpan: {Path.GetFileName(dialog.FileName)}";
    }

    [RelayCommand]
    private async Task LoadProjectAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Buka proyek DokuFoto",
            Filter = "Proyek DokuFoto|*.dokufoto.json;*.json|Semua file|*.*"
        };

        if (dialog.ShowDialog() != true) return;
        CurrentProject = await _repository.LoadProjectAsync(dialog.FileName);
        if (CurrentProject.Pages.Count == 0) CurrentProject.Pages.Add(new DocumentPage());
        ActivePageIndex = 0;
        ActivePage = CurrentProject.Pages[0];
        SyncGridFromPage();
        SelectedCell = null;
        StatusMessage = $"Proyek dibuka: {Path.GetFileName(dialog.FileName)}";
    }

    [RelayCommand]
    private async Task ExportDocxAsync()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Ekspor dokumen Microsoft Word",
            Filter = "Dokumen Word|*.docx",
            FileName = $"Dokumentasi_Setwan_{DateTime.Now:yyyyMMdd_HHmmss}.docx"
        };

        if (dialog.ShowDialog() != true) return;

        IsBusy = true;
        StatusMessage = "Merender crop resolusi tinggi dan membuat .docx...";
        try
        {
            await _docxService.ExportToDocxAsync(CurrentProject, dialog.FileName);
            StatusMessage = $"Dokumen berhasil diekspor: {Path.GetFileName(dialog.FileName)}";
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
        StatusMessage = "Mendeteksi printer...";
        try
        {
            var printers = await _printService.GetAvailablePrintersAsync();
            var target = printers.FirstOrDefault() ?? "Microsoft Print to PDF";
            await _printService.PrintDocumentAsync(CurrentProject, target);
            StatusMessage = $"Dokumen dikirim ke printer: {target}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Gagal mencetak: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ZoomCanvasIn() => CanvasZoom = Math.Min(1.5, CanvasZoom + .1);

    [RelayCommand]
    private void ZoomCanvasOut() => CanvasZoom = Math.Max(.35, CanvasZoom - .1);

    private static PhotoItem CreatePhoto(string path) => new()
    {
        FilePath = path,
        FileName = Path.GetFileName(path),
        CapturedDate = File.GetLastWriteTime(path).ToString("dd MMM yyyy")
    };

    private void SyncGridFromPage()
    {
        GridRows = Math.Max(1, ActivePage.Cells.Select(cell => cell.Row).DefaultIfEmpty(0).Max() + 1);
        GridColumns = Math.Max(1, ActivePage.Cells.Select(cell => cell.Column).DefaultIfEmpty(0).Max() + 1);
        GridHeight = ActivePage.TemplateLayoutId switch
        {
            "hero-1" => 369,
            "split-2" => 245,
            "grid-6-2x3" => 740,
            _ => 500
        };
    }
}
