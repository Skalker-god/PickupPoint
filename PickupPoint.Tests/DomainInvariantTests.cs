using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;
using PickupPoint.Core.Pricing;
using PickupPoint.Core.Services;
using PickupPoint.Core.Storage;
using Xunit;

namespace PickupPoint.Tests;

public sealed class DomainInvariantTests
{
    [Fact]
    public void PickupOrderLine_WithZeroQuantity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PickupOrderLine("SKU-1", 0, 10m));
    }

    [Fact]
    public void PickupOrder_AfterIssue_RejectsNewLines()
    {
        PickupOrder order = NewOrder(1);
        order.AddLine(new PickupOrderLine("SKU-1", 1, 10m));
        order.MarkArrived();
        order.Issue(DateTimeOffset.Now);

        Assert.Throws<InvalidOperationException>(
            () => order.AddLine(new PickupOrderLine("SKU-2", 1, 5m)));
    }

    [Fact]
    public void TotalOf_WithTenPercentDiscount_Returns180()
    {
        PickupOrderService service = new(
            new InMemoryPickupOrderRepository(),
            new DiscountPricingPolicy(0.10m),
            new SilentNotifier());
        PickupOrder order = NewOrder(2);
        order.AddLine(new PickupOrderLine("SKU-1", 2, 100m));

        Assert.Equal(180m, service.TotalOf(order));
    }

    private static PickupOrder NewOrder(int id) =>
        new(id, 100, DateTimeOffset.Now);

    private sealed class SilentNotifier : INotifier
    {
        public void Notify(string email, string subject, string body)
        {
        }
    }
}
