namespace PickupPoint.Legacy;

public sealed class DiscountPolicy
{
    private readonly IReadOnlyList<IDiscountRule> _rules = new IDiscountRule[]
    {
        new VipDiscountRule(),
        new StaffDiscountRule(),
        new LoyalDiscountRule(),
        new NoDiscountRule(),
    };

    public decimal For(decimal subtotal, Recipient recipient)
    {
        foreach (var rule in _rules)
        {
            if (rule.AppliesTo(recipient))
            {
                return rule.AmountFor(subtotal);
            }
        }

        return 0m;
    }
}
