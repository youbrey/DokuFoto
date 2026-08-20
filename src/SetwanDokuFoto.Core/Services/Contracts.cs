using SetwanDokuFoto.Core.Models;

namespace SetwanDokuFoto.Core.Services;

public interface IDocxExportService
{
    Task ExportToDocxAsync(DocumentProject project, string outputPath);
}

public interface IPhotoImportService
{
    Task<List<PhotoItem>> ImportFromDirectoryAsync(string directoryPath);
    Task<PhotoItem?> ImportSinglePhotoAsync(string filePath);
}

public interface IPrintService
{
    Task<List<string>> GetAvailablePrintersAsync();
    Task PrintDocumentAsync(DocumentProject project, string printerName);
}

public interface IProjectRepository
{
    Task SaveProjectAsync(DocumentProject project, string filePath);
    Task<DocumentProject> LoadProjectAsync(string filePath);
}
