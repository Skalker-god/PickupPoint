using PickupPoint.Core.Domain;
using PickupPoint.Core.Orders;
using PickupPoint.Core.Pricing;
using PickupPoint.Core.Reporting;
using PickupPoint.Core.Storage;
using Xunit;

namespace PickupPoint.Tests;

public sealed class PickupOrderStateTests
{
    [Fact]
    public void Cancel_NewOrder_MarksCancelled()
    {
        // Arrange
        PickupOrder order = NewOrder();

        // Act
        order.Cancel();

        // Assert
        Assert.Equal(PickupStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_IssuedOrder_ThrowsInvalidOperation()
    {
        // Arrange
        PickupOrder order = NewIssuedOrder();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void Issue_NewOrder_ThrowsInvalidOperation()
    {
        // Arrange
        PickupOrder order = NewOrder();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(
            () => order.Issue(new DateTimeOffset(2026, 3, 2, 10, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void MarkArrived_OrderWithoutLines_ThrowsInvalidOperation()
    {
        // Arrange
        PickupOrder order = new(
            1, 100, new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero));

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => order.MarkArrived());
    }

    [Fact]
    public void AddLine_AfterArrival_ThrowsInvalidOperation()
    {
        // Arrange
        PickupOrder order = NewOrder();
        order.MarkArrived();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(
            () => order.AddLine(new PickupOrderLine("B-2", 1, 5m)));
    }

    [Fact]
    public void Return_AfterReturnWindow_ThrowsInvalidOperation()
    {
        // Arrange
        PickupOrder order = NewIssuedOrder();
        DateOnly lateDate = new(2026, 3, 16);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(
            () => order.Return(lateDate));
    }

    [Fact]
    public void Return_OnLastDayOfWindow_MarksReturned()
    {
        // Arrange
        PickupOrder order = NewIssuedOrder();

        // Act
        order.Return(new DateOnly(2026, 3, 15));

        // Assert
        Assert.Equal(PickupStatus.Returned, order.Status);
    }

    private static PickupOrder NewOrder()
    {
        PickupOrder order = new(
            1, 100, new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero));
        order.AddLine(new PickupOrderLine("A-1", 1, 10m));
        return order;
    }

    private static PickupOrder NewIssuedOrder()
    {
        PickupOrder order = NewOrder();
        order.MarkArrived();
        order.Issue(new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero));
        return order;
    }
}

public sealed class PricingPolicyTests
{
    [Fact]
    public void PriceOf_VipPolicy_AppliesTenPercent()
    {
        // Arrange
        VipPricingPolicy policy = new();

        // Act
        decimal actual = policy.PriceOf(new PickupOrderLine("A-1", 2, 50m));

        // Assert
        Assert.Equal(90m, actual);
    }

    [Fact]
    public void PriceOf_RegularPolicy_RoundsAwayFromZero()
    {
        // Arrange
        RegularPricingPolicy policy = new();

        // Act
        decimal actual = policy.PriceOf(new PickupOrderLine("A-1", 3, 3.335m));

        // Assert
        Assert.Equal(10.01m, actual);
    }

    [Fact]
    public void PriceOf_NullLine_ThrowsArgumentNull()
    {
        // Arrange
        VipPricingPolicy policy = new();

        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => policy.PriceOf(null!));
    }

    [Fact]
    public void WeekendDiscountOf_Sunday_ReturnsTwoPercent()
    {
        // Arrange
        DateOnly sunday = new(2026, 3, 8);

        // Act
        decimal actual = OrderTotalCalculator.WeekendDiscountOf(1000m, sunday);

        // Assert
        Assert.Equal(20m, actual);
    }
}

public sealed class ProductAndRecipientTests
{
    [Fact]
    public void Issue_PartOfStock_DecreasesStock()
    {
        // Arrange
        Product product = new("A-1", "Книга", 100m, 5);

        // Act
        product.Issue(2);

        // Assert
        Assert.Equal(3, product.Stock);
    }

    [Fact]
    public void Receive_PositiveQuantity_IncreasesStock()
    {
        // Arrange
        Product product = new("A-1", "Книга", 100m, 5);

        // Act
        product.Receive(4);

        // Assert
        Assert.Equal(9, product.Stock);
    }

    [Fact]
    public void Issue_MoreThanStock_ThrowsInvalidOperation()
    {
        // Arrange
        Product product = new("A-1", "Книга", 100m, 5);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => product.Issue(6));
    }

    [Fact]
    public void Receive_ZeroQuantity_ThrowsOutOfRange()
    {
        // Arrange
        Product product = new("A-1", "Книга", 100m, 5);

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => product.Receive(0));
    }

    [Fact]
    public void Constructor_ZeroPrice_ThrowsOutOfRange()
    {
        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Product("A-1", "Книга", 0m, 5));
    }

    [Fact]
    public void Constructor_BlankSku_ThrowsArgumentException()
    {
        // Act + Assert
        Assert.Throws<ArgumentException>(
            () => new Product(" ", "Книга", 100m, 5));
    }

    [Fact]
    public void Recipient_ValidData_KeepsValues()
    {
        // Arrange
        Recipient recipient = new(1, "Іван", "+380501234567", "i@ex.com")
        {
            IsRegular = true
        };

        // Assert
        Assert.Equal("i@ex.com", recipient.Email);
        Assert.True(recipient.IsRegular);
    }

    [Fact]
    public void Recipient_EmailWithoutAt_ThrowsArgumentException()
    {
        // Act + Assert
        Assert.Throws<ArgumentException>(
            () => new Recipient(1, "Іван", "+380501234567", "no-mail"));
    }

    [Fact]
    public void Recipient_ZeroId_ThrowsOutOfRange()
    {
        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Recipient(0, "Іван", "+380501234567", "i@ex.com"));
    }

    [Fact]
    public void ArchivedOrder_ExposesOrderData()
    {
        // Arrange
        PickupOrder order = new(
            5, 100, new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero));
        order.AddLine(new PickupOrderLine("A-1", 1, 10m));
        DateOnly archivedAt = new(2026, 4, 1);

        // Act
        ArchivedPickupOrder archived = new(order, archivedAt);

        // Assert
        Assert.Equal(5, archived.Id);
        Assert.Equal(PickupStatus.New, archived.Status);
        Assert.Single(archived.Lines);
        Assert.Equal(archivedAt, archived.ArchivedAt);
    }

    [Fact]
    public void ArchivedOrder_NullOrder_ThrowsArgumentNull()
    {
        // Act + Assert
        Assert.Throws<ArgumentNullException>(
            () => new ArchivedPickupOrder(null!, new DateOnly(2026, 4, 1)));
    }
}

public sealed class LegacyOrderTests
{
    [Theory]
    [InlineData(1, 1500, true, 1620)]
    [InlineData(1, 6000, false, 6120)]
    [InlineData(1, 500, true, 600)]
    [InlineData(1, 2000, false, 2400)]
    [InlineData(11, 10, false, 12)]
    [InlineData(11, 1, false, 0)]
    [InlineData(0, 0, false, 0)]
    public void CalculateTotal_ByRules_MatchesTable(
        int lineCount, double unitPrice, bool isRegular, double expected)
    {
        // Arrange
        Order order = NewOrder(lineCount, (decimal)unitPrice);

        // Act
        decimal actual = order.CalculateTotal(isRegular);

        // Assert
        Assert.Equal((decimal)expected, actual);
    }

    [Fact]
    public void TryChangeStatus_NewToArrived_ReturnsTrue()
    {
        // Arrange
        Order order = NewOrder(1, 10m);

        // Act
        bool changed = order.TryChangeStatus(1);

        // Assert
        Assert.True(changed);
        Assert.Equal(1, order.Status);
    }

    [Fact]
    public void TryChangeStatus_ArrivedToIssued_ReturnsTrue()
    {
        // Arrange
        Order order = NewOrder(1, 10m);
        order.TryChangeStatus(1);

        // Act
        bool changed = order.TryChangeStatus(2);

        // Assert
        Assert.True(changed);
    }

    [Fact]
    public void TryChangeStatus_NewToCancelled_ReturnsTrue()
    {
        // Arrange
        Order order = NewOrder(1, 10m);

        // Act
        bool changed = order.TryChangeStatus(3);

        // Assert
        Assert.True(changed);
    }

    [Fact]
    public void TryChangeStatus_NewToIssued_ReturnsFalse()
    {
        // Arrange
        Order order = NewOrder(1, 10m);

        // Act
        bool changed = order.TryChangeStatus(2);

        // Assert
        Assert.False(changed);
    }

    [Fact]
    public void IsValid_NormalOrder_ReturnsTrue()
    {
        // Arrange
        Order order = NewOrder(1, 10m);

        // Act
        bool actual = order.IsValid();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValid_OrderWithoutLines_ReturnsFalse()
    {
        // Arrange
        Order order = NewOrder(0, 10m);

        // Act
        bool actual = order.IsValid();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsValid_ShortRecipientName_ReturnsFalse()
    {
        // Arrange
        Order order = new("ORD-1", "Іо");
        order.AddLine("A-1", 1, 10m);

        // Act
        bool actual = order.IsValid();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void FindById_ExistingId_ReturnsOrder()
    {
        // Arrange
        List<Order> orders = new() { new Order("A", "Іван"), new Order("B", "Олег") };

        // Act
        Order? found = Order.FindById(orders, "B");

        // Assert
        Assert.Same(orders[1], found);
    }

    [Fact]
    public void FindById_UnknownId_ReturnsNull()
    {
        // Arrange
        List<Order> orders = new() { new Order("A", "Іван") };

        // Act
        Order? found = Order.FindById(orders, "Z");

        // Assert
        Assert.Null(found);
    }

    [Fact]
    public void BuildReport_OneLine_ContainsLineAndTotal()
    {
        // Arrange
        Order order = NewOrder(1, 10m);

        // Act
        string report = order.BuildReport();

        // Assert
        Assert.StartsWith("Товар: A-1; кількість: 1", report, StringComparison.Ordinal);
        Assert.Contains("Разом: ", report, StringComparison.Ordinal);
    }

    private static Order NewOrder(int lineCount, decimal unitPrice)
    {
        Order order = new("ORD-1", "Іван");
        for (int i = 0; i < lineCount; i++)
        {
            order.AddLine("A-1", 1, unitPrice);
        }

        return order;
    }
}

public sealed class ReportAndStorageTests
{
    [Fact]
    public void BuildCsv_OneOrder_ReturnsHeaderAndRow()
    {
        // Arrange
        PickupOrder order = new(
            5, 100, new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero));
        order.AddLine(new PickupOrderLine("A-1", 2, 10m));
        PickupOrderCsvReport report = new(new StubPricingPolicy(25m));
        string expected = "id;recipient;status;total" + Environment.NewLine
            + "5;100;New;25.00" + Environment.NewLine;

        // Act
        string actual = report.BuildCsv(new List<PickupOrder> { order });

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Constructor_NullPolicy_ThrowsArgumentNull()
    {
        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => new PickupOrderCsvReport(null!));
    }

    [Fact]
    public void Add_DuplicateId_ThrowsInvalidOperation()
    {
        // Arrange
        InMemoryPickupOrderRepository repository = new();
        repository.Add(NewOrder(1));

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => repository.Add(NewOrder(1)));
    }

    [Fact]
    public void GetAll_TwoOrders_ReturnsBoth()
    {
        // Arrange
        InMemoryPickupOrderRepository repository = new();
        repository.Add(NewOrder(1));
        repository.Add(NewOrder(2));

        // Act
        IReadOnlyList<PickupOrder> actual = repository.GetAll();

        // Assert
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        // Arrange
        InMemoryPickupOrderRepository repository = new();

        // Act
        PickupOrder? actual = repository.GetById(99);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void GetById_ZeroId_ThrowsOutOfRange()
    {
        // Arrange
        InMemoryPickupOrderRepository repository = new();

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => repository.GetById(0));
    }

    private static PickupOrder NewOrder(int id) =>
        new(id, 100, new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero));
}
