using PickupPoint.Core.Domain;
using PickupPoint.Core.Pricing;
using Xunit;

namespace PickupPoint.Tests;

public sealed class OrderTotalCalculatorTests
{
    [Fact]
    public void Process_RegularRecipient_Returns1292()
    {
        List<PickupOrderLine> lines = new()
        {
            new PickupOrderLine { Sku = "A1", Quantity = 12, UnitPrice = 100m },
            new PickupOrderLine { Sku = "B2", Quantity = 1, UnitPrice = 250m },
        };
        decimal total = OrderTotalCalculator.Process(
            1, "Іваненко", "i@ex.com", true, lines,
            new DateOnly(2026, 3, 10), null, PickupStatus.New, false,
            60m, out PickupStatus next);
        Assert.Equal(1292.70m, total);
        Assert.Equal(PickupStatus.Arrived, next);
    }

    [Fact]
    public void Process_SmallOrder_AddsDelivery()
    {
        List<PickupOrderLine> lines = new()
        {
            new PickupOrderLine { Sku = "A1", Quantity = 2, UnitPrice = 200m },
        };
        decimal total = OrderTotalCalculator.Process(
            2, "Петренко", "p@ex.com", false, lines,
            new DateOnly(2026, 3, 10), "SALE10", PickupStatus.Arrived,
            false, 60m, out PickupStatus next);
        Assert.Equal(420m, total);
        Assert.Equal(PickupStatus.Issued, next);
    }

    [Fact]
    public void CouponDiscountOf_FixedCouponBelowLimit_ReturnsZero()
    {
        Assert.Equal(0m, OrderTotalCalculator.CouponDiscountOf(900m, "MINUS200"));
    }

    [Fact]
    public void VolumeDiscountOf_AboveLargeLimit_ReturnsTenPercent()
    {
        Assert.Equal(600m, OrderTotalCalculator.VolumeDiscountOf(6000m));
    }

    [Fact]
    public void NextStatusOf_NewWithZeroTotal_StaysNew()
    {
        Assert.Equal(
            PickupStatus.New,
            OrderTotalCalculator.NextStatusOf(PickupStatus.New, 0m));
    }
}
