using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SetwanDokuFoto.Core.Models;

public class DocumentProject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "DOKUMENTASI FOTO KEGIATAN SEKRETARIAT DPRD KOTA BITUNG";
    public string DocumentNumber { get; set; } = "175/SETWAN-DPRD/DOK-FOTO/VIII/2026";
    public string PaperSize { get; set; } = "F4"; // F4 (215 x 330 mm) Standar Setwan
    public string Orientation { get; set; } = "Portrait";
    public PageMargins Margins { get; set; } = new(2.0, 2.0, 2.5, 2.0); // Satuan cm
    public string FontFamily { get; set; } = "Arial";
    public KopSurat KopSurat { get; set; } = new();
    public ObservableCollection<DocumentPage> Pages { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public record PageMargins(double Top, double Bottom, double Left, double Right);

public class KopSurat
{
    public bool Enabled { get; set; } = true;
    public string GovernmentName { get; set; } = "PEMERINTAH KOTA BITUNG";
    public string AgencyName { get; set; } = "DEWAN PERWAKILAN RAKYAT DAERAH";
    public string SubAgencyName { get; set; } = "SEKRETARIAT DEWAN";
    public string Address { get; set; } = "Jl. Sam Ratulangi No. 45, Kel. Bitung Barat Satu, Kec. Maesa, Kota Bitung";
    public string ContactInfo { get; set; } = "Telp: (0438) 21115 | Email: setwan@bitungkota.go.id";
    public string? LogoLeftPath { get; set; }
    public string? LogoRightPath { get; set; }
}

public class DocumentPage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int PageNumber { get; set; } = 1;
    public string Title { get; set; } = "LAPORAN DOKUMENTASI KEGIATAN PENGAWASAN & PERSIDANGAN";
    public string? Subtitle { get; set; } = "Masa Persidangan Ketiga Tahun Sidang 2025/2026";
    public string? ActivityDescription { get; set; }
    public List<MetaTableItem> MetaTable { get; set; } = new();
    public string TemplateLayoutId { get; set; } = "grid-4-2x2";
    public ObservableCollection<CollageCell> Cells { get; set; } = new();
    public SignatureBlock? SignatureBlock { get; set; }
}

public class MetaTableItem
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class CollageCell : INotifyPropertyChanged
{
    private PhotoItem? _photo;
    private string? _caption;
    private bool _showCaption = true;

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int Row { get; set; }
    public int Column { get; set; }
    public int RowSpan { get; set; } = 1;
    public int ColumnSpan { get; set; } = 1;
    public PhotoItem? Photo
    {
        get => _photo;
        set => SetField(ref _photo, value);
    }

    public string? Caption
    {
        get => _caption;
        set => SetField(ref _caption, value);
    }

    public bool ShowCaption
    {
        get => _showCaption;
        set => SetField(ref _showCaption, value);
    }
    public string AspectRatio { get; set; } = "4:3";
    public int Rotation { get; set; } = 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class PhotoItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? CapturedDate { get; set; }
    public string? Category { get; set; }
    public byte[]? ImageBytes { get; set; }
    public PhotoTransform Transform { get; set; } = new();
}

/// <summary>
/// Non-destructive photo placement. Offsets are stored relative to the frame
/// size, Scale is a multiplier on top of the automatic cover scale, and
/// Rotation is expressed in degrees. The source bitmap is never modified.
/// </summary>
public sealed class PhotoTransform : INotifyPropertyChanged
{
    private double _offsetX;
    private double _offsetY;
    private double _scale = 1;
    private double _rotation;
    private bool _flipHorizontal;
    private bool _flipVertical;

    public double OffsetX
    {
        get => _offsetX;
        set => SetField(ref _offsetX, value);
    }

    public double OffsetY
    {
        get => _offsetY;
        set => SetField(ref _offsetY, value);
    }

    public double Scale
    {
        get => _scale;
        set => SetField(ref _scale, Math.Max(1, value));
    }

    public double Rotation
    {
        get => _rotation;
        set => SetField(ref _rotation, NormalizeAngle(value));
    }

    public bool FlipHorizontal
    {
        get => _flipHorizontal;
        set => SetField(ref _flipHorizontal, value);
    }

    public bool FlipVertical
    {
        get => _flipVertical;
        set => SetField(ref _flipVertical, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public PhotoTransformSnapshot Snapshot() =>
        new(OffsetX, OffsetY, Scale, Rotation, FlipHorizontal, FlipVertical);

    public void Restore(PhotoTransformSnapshot snapshot)
    {
        OffsetX = snapshot.OffsetX;
        OffsetY = snapshot.OffsetY;
        Scale = snapshot.Scale;
        Rotation = snapshot.Rotation;
        FlipHorizontal = snapshot.FlipHorizontal;
        FlipVertical = snapshot.FlipVertical;
    }

    public void Reset()
    {
        OffsetX = 0;
        OffsetY = 0;
        Scale = 1;
        Rotation = 0;
        FlipHorizontal = false;
        FlipVertical = false;
    }

    private static double NormalizeAngle(double value)
    {
        var normalized = value % 360;
        return normalized < 0 ? normalized + 360 : normalized;
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public readonly record struct PhotoTransformSnapshot(
    double OffsetX,
    double OffsetY,
    double Scale,
    double Rotation,
    bool FlipHorizontal,
    bool FlipVertical);

public class SignatureBlock
{
    public bool Enabled { get; set; } = true;
    public string CityAndDate { get; set; } = "Bitung, 17 Agustus 2026";
    public string RoleTitle { get; set; } = "Kepala Bagian Persidangan dan Risalah";
    public string OfficerName { get; set; } = "SEKRETARIAT DPRD KOTA BITUNG";
    public string Nip { get; set; } = "NIP. 19800512 200501 1 008";
}
