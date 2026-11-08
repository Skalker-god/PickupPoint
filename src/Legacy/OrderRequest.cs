using System.Collections.Generic;

namespace PickupPoint.Legacy;

public sealed record OrderRequest(
    int OrderId,
    Recipient Recipient,
    IReadOnlyList<PickupLine>? Items,
    OrderState State,
    string Currency,
    bool SendMail);
