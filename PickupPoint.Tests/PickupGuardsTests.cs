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
    public void EnsureLineValid_NullLine_ThrowsArgumentNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => PickupGuards.EnsureLineValid(null!));
    }
}
