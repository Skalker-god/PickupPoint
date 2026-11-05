using PickupPoint.Core.Domain;
using PickupPoint.Core.Pricing;
using Xunit;

namespace PickupPoint.Tests;

public sealed class PickupGuardsTests
{
    [Fact]
    public void EnsureLinesValid_NoLines_ThrowsArgument()
    {
        Assert.Throws<ArgumentException>(() =>
            PickupGuards.EnsureLinesValid(new List<PickupOrderLine>()));
    }

    [Fact]
    public void EnsureLineValid_ZeroQuantity_ThrowsRange()
    {
        PickupOrderLine line = new()
        {
            Sku = "A1",
            Quantity = 0,
            UnitPrice = 10m,
        };
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PickupGuards.EnsureLineValid(line));
    }
}
