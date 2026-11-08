namespace PickupPoint.Legacy;

public interface IDiscountRule
{
    bool AppliesTo(Recipient recipient);

    decimal AmountFor(decimal subtotal);
}
