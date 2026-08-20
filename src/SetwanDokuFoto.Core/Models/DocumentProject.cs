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
    public List<DocumentPage> Pages { get; set; } = new();
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
    public List<CollageCell> Cells { get; set; } = new();
    public SignatureBlock? SignatureBlock { get; set; }
}

public class MetaTableItem
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class CollageCell
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int Row { get; set; }
    public int Column { get; set; }
    public int RowSpan { get; set; } = 1;
    public int ColumnSpan { get; set; } = 1;
    public PhotoItem? Photo { get; set; }
    public string? Caption { get; set; }
    public bool ShowCaption { get; set; } = true;
    public string AspectRatio { get; set; } = "4:3";
    public int Rotation { get; set; } = 0;
}

public class PhotoItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? CapturedDate { get; set; }
    public string? Category { get; set; }
    public byte[]? ImageBytes { get; set; }
}

public class SignatureBlock
{
    public bool Enabled { get; set; } = true;
    public string CityAndDate { get; set; } = "Bitung, 17 Agustus 2026";
    public string RoleTitle { get; set; } = "Kepala Bagian Persidangan dan Risalah";
    public string OfficerName { get; set; } = "SEKRETARIAT DPRD KOTA BITUNG";
    public string Nip { get; set; } = "NIP. 19800512 200501 1 008";
}
