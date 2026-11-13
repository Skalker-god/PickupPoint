using PickupPoint.App;
using PickupPoint.Core.Domain;
using PickupPoint.Core.Errors;
using Xunit;

namespace PickupPoint.Tests;

public sealed class PickupOrderLineParserTests
{
    [Theory]
    [InlineData("", "1", "10")]
    [InlineData("   ", "1", "10")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU", "1", "10")]
    [InlineData("A-1", "0", "10")]
    [InlineData("A-1", "-2", "10")]
    [InlineData("A-1", "1001", "10")]
    [InlineData("A-1", "abc", "10")]
    [InlineData("A-1", "", "10")]
    [InlineData("A-1", "1", "-10.50")]
    [InlineData("A-1", "1", "250,50")]
    [InlineData("A-1", "1", "1000000.01")]
    [InlineData("A-1", "1", "abc")]
    public void Parse_InvalidInput_ReturnsFail(
        string sku,
        string quantity,
        string price)
    {
        // Act
        Result<PickupOrderLine> result =
            PickupOrderLineParser.Parse(sku, quantity, price);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrEmpty(result.Error));
    }

    [Fact]
    public void Parse_ValidInput_TrimsSkuAndReturnsLine()
    {
        // Act
        Result<PickupOrderLine> result =
            PickupOrderLineParser.Parse(" SKU-1 ", "1000", "10.50");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("SKU-1", result.Value!.Sku);
        Assert.Equal(1000, result.Value.Quantity);
        Assert.Equal(10.50m, result.Value.UnitPrice);
    }

    [Theory]
    [InlineData("12345678901234567890", "1", "0")]
    [InlineData("A-1", "1000", "1000000")]
    [InlineData("A-1", "1", "0")]
    public void Parse_BoundaryValues_ReturnsSuccess(
        string sku,
        string quantity,
        string price)
    {
        // Act
        Result<PickupOrderLine> result =
            PickupOrderLineParser.Parse(sku, quantity, price);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
