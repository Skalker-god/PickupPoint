using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;
using PickupPoint.Core.Errors;
using PickupPoint.Core.Pricing;
using PickupPoint.Core.Services;
using PickupPoint.Core.Storage;
using Xunit;

namespace PickupPoint.Tests;

public sealed class ServiceErrorTests
{
    [Fact]
    public void AddLine_WithNullLine_ThrowsArgumentNull()
    {
        (PickupOrderService service, PickupOrder order) = NewService();
        service.Place(order, "i@ex.com");

        Assert.Throws<ArgumentNullException>(
            () => service.AddLine(order.Id, null!));
    }

    [Fact]
    public void AddLine_ForMissingOrder_ThrowsDomainRule()
    {
        (PickupOrderService service, _) = NewService();

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => service.AddLine(99, new PickupOrderLine("SKU-1", 1, 10m)));

        Assert.Equal("order.exists", exception.Rule);
        Assert.Contains("99", exception.Message);
    }

    [Fact]
    public void AddLine_AfterIssue_ThrowsEditableRule()
    {
        (PickupOrderService service, PickupOrder order) = NewService();
        service.Place(order, "i@ex.com");
        service.AddLine(order.Id, new PickupOrderLine("SKU-1", 1, 10m));
        order.MarkArrived();
        order.Issue(DateTimeOffset.Now);

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => service.AddLine(
                order.Id,
                new PickupOrderLine("SKU-2", 1, 5m)));

        Assert.Equal("order.editable", exception.Rule);
    }

    [Fact]
    public void AddLine_ForNewOrder_AddsTypedLine()
    {
        (PickupOrderService service, PickupOrder order) = NewService();
        service.Place(order, "i@ex.com");
        service.AddLine(order.Id, new PickupOrderLine("SKU-1", 3, 20m));

        Assert.Single(order.Lines);
        Assert.Equal(3, order.Lines[0].Quantity);
    }

    private static (PickupOrderService Service, PickupOrder Order) NewService()
    {
        InMemoryPickupOrderRepository repository = new();
        PickupOrderService service = new(
            repository,
            new RegularPricingPolicy(),
            new SilentNotifier());
        return (service, new PickupOrder(1, 100, DateTimeOffset.Now));
    }

    private sealed class SilentNotifier : INotifier
    {
        public void Notify(string email, string subject, string body)
        {
        }
    }
}
