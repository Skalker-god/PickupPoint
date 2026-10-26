namespace PickupPoint.Core.Domain;

/// <summary>Стан замовлення на видачу в його життєвому циклі.</summary>
public enum PickupStatus
{
    /// <summary>Замовлення зареєстровано.</summary>
    New = 0,

    /// <summary>Замовлення прибуло до пункту видачі.</summary>
    Arrived = 1,

    /// <summary>Замовлення видано одержувачу.</summary>
    Issued = 2,

    /// <summary>Замовлення повернено відправнику.</summary>
    Returned = 3,

    /// <summary>Замовлення скасовано.</summary>
    Cancelled = 4,
}
