using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;

namespace PickupPoint.Tests;

internal sealed class FakeOrderRepository : IPickupOrderRepository
{
    private readonly Dictionary<int, PickupOrder> _items = new();

    public int AddCalls { get; private set; }

    public void Add(PickupOrder order)
    {
        AddCalls++;
        _items[order.Id] = order;
    }

    public PickupOrder? GetById(int id) =>
        _items.TryGetValue(id, out PickupOrder? found) ? found : null;

    public IReadOnlyList<PickupOrder> GetAll() => _items.Values.ToList();
}

internal sealed class StubPricingPolicy : IPricingPolicy
{
    private readonly decimal _price;

    public StubPricingPolicy(decimal price) => _price = price;

    public decimal PriceOf(PickupOrderLine line) => _price;
}

internal sealed class RecordingNotifier : INotifier
{
    private readonly List<(string Email, string Subject, string Body)> _sent = new();

    public IReadOnlyList<(string Email, string Subject, string Body)> Sent => _sent;

    public void Notify(string email, string subject, string body) =>
        _sent.Add((email, subject, body));
}