using System.Text;

namespace PickupPoint.Legacy;

public sealed class OperationLog
{
    private readonly List<string> _entries = new();

    public void Add(string entry)
    {
        _entries.Add(entry);
    }

    public string Dump()
    {
        var sb = new StringBuilder();
        foreach (var entry in _entries)
        {
            sb.Append(entry).Append('\n');
        }

        return sb.ToString();
    }
}
