using PickupPoint.Core.Pricing;
using Xunit;

namespace PickupPoint.Tests;

public sealed class VolumeDiscountTests
{
    [Fact]
    public void VolumeDiscountOf_ZeroAmount_ReturnsZero()
    {
        // Arrange
        decimal amount = 0m;

        // Act
        decimal actual = OrderTotalCalculator.VolumeDiscountOf(amount);

        // Assert
        Assert.Equal(0m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_SmallOrder_ReturnsZero()
    {
        // Arrange
        decimal amount = 1000m;

        // Act
        decimal actual = OrderTotalCalculator.VolumeDiscountOf(amount);

        // Assert
        Assert.Equal(0m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_JustBelowMediumLimit_ReturnsZero()
    {
        // Arrange
        decimal amount = 1999.99m;

        // Act
        decimal actual = OrderTotalCalculator.VolumeDiscountOf(amount);

        // Assert
        Assert.Equal(0m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_AtMediumLimit_ReturnsZero()
    {
        // Arrange
        decimal amount = 2000m;

        // Act
        decimal actual = OrderTotalCalculator.VolumeDiscountOf(amount);

        // Assert
        Assert.Equal(0m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_JustAboveMediumLimit_ReturnsFivePercent()
    {
        // Arrange
        decimal amount = 2001m;

        // Act
        decimal actual = OrderTotalCalculator.VolumeDiscountOf(amount);

        // Assert
        Assert.Equal(100.05m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_AtLargeLimit_ReturnsFivePercent()
    {
        // Arrange
        decimal amount = 5000m;

        // Act
        decimal actual = OrderTotalCalculator.VolumeDiscountOf(amount);

        // Assert
        Assert.Equal(250m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_JustAboveLargeLimit_ReturnsTenPercent()
    {
        // Arrange
        decimal amount = 5001m;

        // Act
        decimal actual = OrderTotalCalculator.VolumeDiscountOf(amount);

        // Assert
        Assert.Equal(500.10m, actual);
    }
}