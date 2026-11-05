using PickupPoint.Core.Domain;
using PickupPoint.Core.Pricing;
using Xunit;

namespace PickupPoint.Tests;

public sealed class OrderTotalCalculatorTests
{
    [Fact]
    public void Calculate_RegularRecipient_Returns1292()
    {
        TotalRequest request = new(
            OrderId: 1,
            Recipient: new RecipientInfo("Іваненко", "i@ex.com", true),
            Lines: new List<PickupOrderLine>
            {
                new() { Sku = "A1", Quantity = 12, UnitPrice = 100m },
                new() { Sku = "B2", Quantity = 1, UnitPrice = 250m },
            },
            CreatedOn: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: PickupStatus.New,
            DeliveryPrice: 60m);
        TotalResult result = OrderTotalCalculator.Calculate(request);
        Assert.Equal(1292.70m, result.Total);
        Assert.Equal(PickupStatus.Arrived, result.Status);
    }

    [Fact]
    public void Calculate_SmallOrder_AddsDelivery()
    {
        TotalRequest request = new(
            OrderId: 2,
            Recipient: new RecipientInfo("Петренко", "p@ex.com", false),
            Lines: new List<PickupOrderLine>
            {
                new() { Sku = "A1", Quantity = 2, UnitPrice = 200m },
            },
            CreatedOn: new DateOnly(2026, 3, 10),
            CouponCode: "SALE10",
            Status: PickupStatus.Arrived,
            DeliveryPrice: 60m);
        TotalResult result = OrderTotalCalculator.Calculate(request);
        Assert.Equal(420m, result.Total);
        Assert.Equal(PickupStatus.Issued, result.Status);
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
