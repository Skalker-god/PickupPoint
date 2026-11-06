using PickupPoint.Core.Domain;
using PickupPoint.Core.Pricing;
using PickupPoint.Core.Reporting;
using Xunit;

namespace PickupPoint.Tests;

public sealed class OrderReportTests
{
    [Fact]
    public void TextOf_ContainsTotalLine()
    {
        TotalRequest request = BuildSampleRequest();
        TotalResult result = OrderTotalCalculator.Calculate(request);
        string text = OrderReport.TextOf(request, result);
        Assert.Contains("Разом: 1292.70", text);
    }

    [Fact]
    public void Print_WritesReportToOutput()
    {
        StringWriter writer = new();
        OrderReportPrinter printer = new(writer);
        printer.Print("Разом: 10.00");
        Assert.Contains("10.00", writer.ToString());
    }

    private static TotalRequest BuildSampleRequest()
    {
        return new TotalRequest(
            OrderId: 1,
            Recipient: new RecipientInfo("Іваненко", "i@ex.com", true),
            Lines: new List<PickupOrderLine>
            {
                new("A1", 12, 100m),
                new("B2", 1, 250m),
            },
            CreatedOn: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: PickupStatus.New,
            DeliveryPrice: 60m);
    }
}
