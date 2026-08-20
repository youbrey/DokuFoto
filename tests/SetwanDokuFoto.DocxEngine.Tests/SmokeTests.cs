using FluentAssertions;
using SetwanDokuFoto.Core.Models;
using SetwanDokuFoto.DocxEngine;
using Xunit;

namespace SetwanDokuFoto.DocxEngine.Tests;

public class DocxEngineSmokeTests
{
    [Fact]
    public async Task ExportToDocxAsync_Should_Create_Valid_Docx_File()
    {
        // Arrange
        var service = new DocxExportService();
        var project = new DocumentProject();
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_export_{Guid.NewGuid()}.docx");

        try
        {
            // Act
            await service.ExportToDocxAsync(project, tempFile);

            // Assert
            File.Exists(tempFile).Should().BeTrue();
            var fileInfo = new FileInfo(tempFile);
            fileInfo.Length.Should().BeGreaterThan(100);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
