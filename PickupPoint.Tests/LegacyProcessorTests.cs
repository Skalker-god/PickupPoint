using PickupPoint.Core.Domain;
using PickupPoint.Core.Legacy;
using Xunit;

namespace PickupPoint.Tests;

public sealed class LegacyProcessorTests
{
    [Fact]
    public void Process_RegularRecipient_Returns1292()
    {
        List<PickupOrderLine> lines = new()
        {
            new PickupOrderLine { Sku = "A1", Quantity = 12, UnitPrice = 100m },
            new PickupOrderLine { Sku = "B2", Quantity = 1, UnitPrice = 250m },
        };
        decimal total = LegacyProcessor.Process(
            1, "Іваненко", "i@ex.com", true, lines,
            new DateOnly(2026, 3, 10), "", "New", false,
            60m, out string next);
        Assert.Equal(1292.70m, total);
        Assert.Equal("Arrived", next);
    }

    [Fact]
    public void Process_SmallOrder_AddsDelivery()
    {
        List<PickupOrderLine> lines = new()
        {
            new PickupOrderLine { Sku = "A1", Quantity = 2, UnitPrice = 200m },
        };
        decimal total = LegacyProcessor.Process(
            2, "Петренко", "p@ex.com", false, lines,
            new DateOnly(2026, 3, 10), "SALE10", "Arrived",
            false, 60m, out string next);
        Assert.Equal(420m, total);
        Assert.Equal("Issued", next);
    }

    [Fact]
    public void Process_NoLines_ReturnsMinusOne()
    {
        decimal total = LegacyProcessor.Process(
            3, "Коваль", "k@ex.com", false,
            new List<PickupOrderLine>(),
            new DateOnly(2026, 3, 10), "", "New", false,
            60m, out string next);
        Assert.Equal(-1m, total);
        Assert.Equal("New", next);
    }
}
