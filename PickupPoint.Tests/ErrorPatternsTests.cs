using PickupPoint.Core.Domain;
using PickupPoint.Core.Errors;
using PickupPoint.Core.Services;
using PickupPoint.Core.Storage;
using Xunit;

namespace PickupPoint.Tests;

public sealed class ErrorPatternsTests
{
    [Fact]
    public void Load_ForMissingOrder_ThrowsInsteadOfReturningNull()
    {
        ErrorPatternsDemo demo = new(new InMemoryPickupOrderRepository());

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => demo.Load(99));

        Assert.Equal("order.exists", exception.Rule);
    }

    [Fact]
    public void Load_ForExistingOrder_ReturnsIt()
    {
        InMemoryPickupOrderRepository repository = new();
        PickupOrder order = NewStoredOrder(repository);
        ErrorPatternsDemo demo = new(repository);

        Assert.Same(order, demo.Load(order.Id));
    }

    [Fact]
    public void Cancel_ForIssuedOrder_WrapsCauseAndKeepsInnerException()
    {
        InMemoryPickupOrderRepository repository = new();
        PickupOrder order = NewStoredOrder(repository);
        order.MarkArrived();
        order.Issue(DateTimeOffset.Now);
        ErrorPatternsDemo demo = new(repository);

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => demo.Cancel(order.Id, "пізно"));

        Assert.Equal("order.cancellable", exception.Rule);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    [Fact]
    public void CountValidQuantities_MixedRows_ReturnsTwo()
    {
        string[] rows = { "2", "дві", "-1", "5" };
        int count = ErrorPatternsDemo.CountValidQuantities(rows);

        Assert.Equal(2, count);
    }

    [Fact]
    public void ExportReport_WritesFileAndReleasesHandle()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"pickup-report-{Guid.NewGuid():N}.txt");
        PickupOrder order = new(1, 100, DateTimeOffset.Now);
        order.AddLine(new PickupOrderLine("SKU-1", 2, 250m));
        try
        {
            ErrorPatternsDemo.ExportReport(path, order);

            string text = File.ReadAllText(path);
            Assert.Contains("Замовлення 1", text);
            File.Delete(path);
            Assert.False(File.Exists(path));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static PickupOrder NewStoredOrder(
        InMemoryPickupOrderRepository repository)
    {
        PickupOrder order = new(1, 100, DateTimeOffset.Now);
        order.AddLine(new PickupOrderLine("SKU-1", 1, 10m));
        repository.Add(order);
        return order;
    }
}
