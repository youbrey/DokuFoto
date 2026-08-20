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

    [Fact]
    public void CoverScale_Should_Always_Fill_The_Frame()
    {
        var scale = CanvaCropMath.CalculateCoverScale(500, 500, 4000, 3000);

        (4000 * scale).Should().BeGreaterThanOrEqualTo(500);
        (3000 * scale).Should().BeGreaterThanOrEqualTo(500);
    }

    [Fact]
    public void Rotated_Cover_Should_Increase_Minimum_Zoom_When_Needed()
    {
        var multiplier = CanvaCropMath.CalculateRotationCoverMultiplier(
            frameWidth: 500,
            frameHeight: 300,
            coveredImageWidth: 500,
            coveredImageHeight: 375,
            rotationDegrees: 45);

        multiplier.Should().BeGreaterThan(1);
    }

    [Fact]
    public void Crop_Reset_Should_Restore_NonDestructive_Defaults()
    {
        var transform = new PhotoTransform
        {
            OffsetX = .2,
            OffsetY = -.1,
            Scale = 1.7,
            Rotation = 27,
            FlipHorizontal = true
        };

        transform.Reset();

        transform.OffsetX.Should().Be(0);
        transform.OffsetY.Should().Be(0);
        transform.Scale.Should().Be(1);
        transform.Rotation.Should().Be(0);
        transform.FlipHorizontal.Should().BeFalse();
    }
}
