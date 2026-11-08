namespace PickupPoint.Legacy;

internal abstract class DiscountRule : IDiscountRule
{
    protected DiscountRule(decimal rate)
    {
        Rate = rate;
    }

    protected decimal Rate { get; }

    public abstract bool AppliesTo(Recipient recipient);

    public decimal AmountFor(decimal subtotal)
    {
        decimal amount = subtotal * Rate;
        return amount > PricingRules.MaxDiscount
            ? PricingRules.MaxDiscount
            : amount;
    }
}

internal sealed class VipDiscountRule : DiscountRule
{
    public VipDiscountRule()
        : base(PricingRules.VipRate)
    {
    }

    public override bool AppliesTo(Recipient recipient)
    {
        return recipient.Kind == RecipientKind.Vip;
    }
}

internal sealed class StaffDiscountRule : DiscountRule
{
    public StaffDiscountRule()
        : base(PricingRules.StaffRate)
    {
    }

    public override bool AppliesTo(Recipient recipient)
    {
        return recipient.Kind == RecipientKind.Staff;
    }
}

internal sealed class LoyalDiscountRule : DiscountRule
{
    public LoyalDiscountRule()
        : base(PricingRules.LoyalRate)
    {
    }

    public override bool AppliesTo(Recipient recipient)
    {
        return recipient.IsLoyal;
    }
}

internal sealed class NoDiscountRule : IDiscountRule
{
    public bool AppliesTo(Recipient recipient)
    {
        return true;
    }

    public decimal AmountFor(decimal subtotal)
    {
        return 0m;
    }
}
