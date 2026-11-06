using PickupPoint.Core.Abstractions;

internal sealed class ConsoleNotifier : INotifier
{
    public void Notify(string email, string subject, string body)
    {
        Console.WriteLine($"Сповіщення для {email}: {subject}. {body}");
    }
}
