namespace PickupPoint.Core.Domain;
/// <summary>Стан замовлення на видачу в його життєвому циклі.</summary>
public enum PickupStatus
{
    New = 0,
    Arrived = 1,
    Issued = 2,
    Returned = 3,
    Cancelled = 4,
}
