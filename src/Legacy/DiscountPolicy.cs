namespace PickupPoint.Legacy;

public sealed class DiscountPolicy
{
    public decimal For(decimal subtotal, Recipient recipient)
    {
        decimal discount = 0m;
        if (recipient.Kind == RecipientKind.Vip)
        {
            discount = subtotal * PricingRules.VipRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        else if (recipient.Kind == RecipientKind.Staff)
        {
            discount = subtotal * PricingRules.StaffRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        else if (recipient.DoneCount > PricingRules.LoyalOrders)
        {
            discount = subtotal * PricingRules.LoyalRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }

        return discount;
    }
}
