using PickupPoint.Core.Domain;
using PickupPoint.Core.Pricing;
using PickupPoint.Core.Reporting;

TotalRequest request = BuildSampleRequest();
TotalResult result = OrderTotalCalculator.Calculate(request);
string report = OrderReport.TextOf(request, result);
new OrderReportPrinter(Console.Out).Print(report);

static TotalRequest BuildSampleRequest()
{
    return new TotalRequest(
        OrderId: 1,
        Recipient: new RecipientInfo("Іваненко", "i@ex.com", true),
        Lines: new List<PickupOrderLine>
        {
            new() { Sku = "SKU00001", Quantity = 12, UnitPrice = 100m },
            new() { Sku = "SKU00002", Quantity = 1, UnitPrice = 250m },
        },
        CreatedOn: new DateOnly(2026, 3, 10),
        CouponCode: null,
        Status: PickupStatus.New,
        DeliveryPrice: 60m);
}
