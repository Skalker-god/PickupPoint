using PickupPoint.Core.Reporting;
using Xunit;

namespace PickupPoint.Tests;

public sealed class ReportBuilderTests
{
    private static readonly string[] _twoRows = { "a", "b" };

    [Fact]
    public void Build_TwoRows_ReturnsUpperTitleAndRows()
    {
        // Arrange
        ReportBuilder builder = new();
        string nl = Environment.NewLine;

        // Act
        string actual = builder.Build(_twoRows);

        // Assert
        Assert.Equal("ЗВІТ" + nl + "a" + nl + "b" + nl, actual);
    }

    [Fact]
    public void Build_NullRows_ThrowsArgumentNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ReportBuilder().Build(null!));
    }
}
