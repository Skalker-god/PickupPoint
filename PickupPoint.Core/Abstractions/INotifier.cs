namespace PickupPoint.Core.Abstractions;

/// <summary>Сповіщувач одержувача про подію замовлення.</summary>
public interface INotifier
{
    /// <summary>Надсилає сповіщення одержувачу.</summary>
    /// <param name="email">Пошта одержувача.</param>
    /// <param name="subject">Тема повідомлення.</param>
    /// <param name="body">Текст повідомлення.</param>
    void Notify(string email, string subject, string body);
}
