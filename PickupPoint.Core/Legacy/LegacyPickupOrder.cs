namespace PickupPoint.Core.Legacy;

public class LegacyPickupLine
{
    public string Sku { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}

public class LegacyPickupOrder
{
    public int Id { get; set; }

    public int Status { get; set; }

    public decimal Total { get; set; }

    public string Email { get; set; } = string.Empty;

    public List<LegacyPickupLine> Lines { get; set; } = new();

    public virtual void AddLine(LegacyPickupLine line) => Lines.Add(line);
}

public class ArchivedPickupOrder : LegacyPickupOrder
{
    public ArchivedPickupOrder(int id) => Id = id;

    public override void AddLine(LegacyPickupLine line)
    {
        throw new NotSupportedException("Архів змінювати не можна");
    }
}
