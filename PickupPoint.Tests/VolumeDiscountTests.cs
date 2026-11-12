using PickupPoint.Core.Pricing;
using Xunit;

namespace PickupPoint.Tests;

public sealed class VolumeDiscountTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1000, 0)]
    [InlineData(1999.99, 0)]
    [InlineData(2000, 0)]
    [InlineData(2001, 100.05)]
    [InlineData(5000, 250)]
    [InlineData(5001, 500.1)]
    public void VolumeDiscountOf_Boundaries_MatchesTable(
        double amount, double expected)
    {
        // Arrange
        decimal input = (decimal)amount;

        // Act
        decimal actual = OrderTotalCalculator.VolumeDiscountOf(input);

        // Assert
        Assert.Equal((decimal)expected, actual);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1000)]
    public void VolumeDiscountOf_NegativeAmount_ThrowsOutOfRange(
        double amount)
    {
        // Arrange
        decimal input = (decimal)amount;

        // Act
        ArgumentOutOfRangeException ex =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => OrderTotalCalculator.VolumeDiscountOf(input));

        // Assert
        Assert.Equal("amount", ex.ParamName);
    }

    [Fact]
    public void VolumeDiscountOf_HugeOrder_IsCappedAtMax()
    {
        // Arrange
        decimal amount = 20000m;

        // Act
        decimal actual = OrderTotalCalculator.VolumeDiscountOf(amount);

        // Assert
        Assert.Equal(1000m, actual);
    }
}