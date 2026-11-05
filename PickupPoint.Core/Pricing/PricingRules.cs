namespace PickupPoint.Core.Pricing;

internal static class PricingRules
{
    public const int BulkQuantity = 10;

    public const decimal BulkRate = 0.05m;

    public const decimal RegularRecipientRate = 0.07m;

    public const decimal LargeOrderFrom = 5000m;

    public const decimal LargeOrderRate = 0.10m;

    public const decimal MediumOrderFrom = 2000m;

    public const decimal MediumOrderRate = 0.05m;

    public const decimal WeekendRate = 0.02m;

    public const decimal CouponSaleRate = 0.10m;

    public const decimal CouponFixed = 200m;

    public const decimal CouponFixedFrom = 1000m;

    public const decimal FreeDeliveryFrom = 1000m;

    public const int CurrencyDigits = 2;

    public const string SaleCoupon = "SALE10";

    public const string FixedCoupon = "MINUS200";
}
