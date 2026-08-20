using FluentAssertions;
using SetwanDokuFoto.Core.Models;
using Xunit;

namespace SetwanDokuFoto.Core.Tests;

public class CoreModelSmokeTests
{
    [Fact]
    public void DocumentProject_Should_Initialize_With_F4_PaperSize_Default()
    {
        // Arrange & Act
        var project = new DocumentProject();

        // Assert
        project.PaperSize.Should().Be("F4");
        project.Orientation.Should().Be("Portrait");
        project.Margins.Top.Should().Be(2.0);
        project.Margins.Left.Should().Be(2.5); // Binding margin for Setwan
        project.KopSurat.AgencyName.Should().Contain("DEWAN PERWAKILAN RAKYAT DAERAH");
    }

    [Fact]
    public void DocumentPage_Should_Add_And_Hold_CollageCells()
    {
        // Arrange
        var page = new DocumentPage { Title = "DOKUMENTASI SIDANG PARIPURNA" };

        // Act
        page.Cells.Add(new CollageCell { Row = 0, Column = 0, Caption = "Foto Pembukaan Sidang" });

        // Assert
        page.Cells.Should().HaveCount(1);
        page.Cells[0].Caption.Should().Be("Foto Pembukaan Sidang");
    }
}
