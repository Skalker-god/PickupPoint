namespace PickupPoint.Legacy;

public sealed class DiscountPolicy
{
    public decimal For(decimal subtotal, RecipientKind kind, int done)
    {
        decimal discount = 0m;
        if (kind == RecipientKind.Vip)
        {
            discount = subtotal * PricingRules.VipRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        else if (kind == RecipientKind.Staff)
        {
            discount = subtotal * PricingRules.StaffRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        else if (done > PricingRules.LoyalOrders)
        {
            discount = subtotal * PricingRules.LoyalRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }

        return discount;
    }
}
