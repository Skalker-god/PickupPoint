using PickupPoint.Core.Domain;
using PickupPoint.Core.Services;
using Xunit;

namespace PickupPoint.Tests;

public sealed class OrderServiceDoublesTests
{
    [Fact]
    public void TotalOf_TwoLines_SumsPricesFromPolicy()
    {
        // Arrange
        PickupOrder order = NewOrder(7);
        order.AddLine(new PickupOrderLine("A-1", 2, 10m));
        order.AddLine(new PickupOrderLine("B-2", 1, 30m));
        PickupOrderService service = new(
            new FakeOrderRepository(),
            new StubPricingPolicy(25m),
            new RecordingNotifier());

        // Act
        decimal actual = service.TotalOf(order);

        // Assert
        Assert.Equal(50m, actual);
    }

    [Fact]
    public void TotalOf_OrderWithoutLines_ReturnsZero()
    {
        // Arrange
        PickupOrder order = NewOrder(8);
        PickupOrderService service = new(
            new FakeOrderRepository(),
            new StubPricingPolicy(25m),
            new RecordingNotifier());

        // Act
        decimal actual = service.TotalOf(order);

        // Assert
        Assert.Equal(0m, actual);
    }

    [Fact]
    public void Place_NewOrder_StoresOrderInRepository()
    {
        // Arrange
        FakeOrderRepository repository = new();
        PickupOrderService service = new(
            repository,
            new StubPricingPolicy(1m),
            new RecordingNotifier());
        PickupOrder order = NewOrder(1);

        // Act
        service.Place(order, "i@ex.com");

        // Assert
        Assert.Same(order, repository.GetById(1));
        Assert.Equal(1, repository.AddCalls);
    }

    [Fact]
    public void Place_NewOrder_NotifiesRecipientOnce()
    {
        // Arrange
        RecordingNotifier notifier = new();
        PickupOrderService service = new(
            new FakeOrderRepository(),
            new StubPricingPolicy(1m),
            notifier);

        // Act
        service.Place(NewOrder(1), "i@ex.com");

        // Assert
        Assert.Single(notifier.Sent);
        Assert.Equal("i@ex.com", notifier.Sent[0].Email);
        Assert.Equal("Замовлення № 1", notifier.Sent[0].Subject);
    }

    private static PickupOrder NewOrder(int id) =>
        new(id, 100, new DateTimeOffset(2026, 3, 10, 12, 0, 0, TimeSpan.Zero));
}
