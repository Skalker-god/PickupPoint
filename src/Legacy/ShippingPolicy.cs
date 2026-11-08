namespace PickupPoint.Legacy;

public sealed class ShippingPolicy
{
    public decimal For(decimal payable)
    {
        return payable < PricingRules.FreeShipFrom
            ? PricingRules.ShipCost
            : 0m;
    }
}
