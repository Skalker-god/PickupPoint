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
            new("SKU00001", 12, 100m),
            new("SKU00002", 1, 250m),
        },
        CreatedOn: new DateOnly(2026, 3, 10),
        CouponCode: null,
        Status: PickupStatus.New,
        DeliveryPrice: 60m);
}
