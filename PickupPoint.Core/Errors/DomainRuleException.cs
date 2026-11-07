namespace PickupPoint.Core.Errors;

/// <summary>
/// Порушення правила предметної області. Містить машинозчитуваний
/// ідентифікатор правила та може зберігати первинний виняток.
/// </summary>
public sealed class DomainRuleException : Exception
{
    /// <summary>Створює виняток із правилом і повідомленням.</summary>
    /// <param name="rule">Ідентифікатор правила, наприклад <c>order.exists</c>.</param>
    /// <param name="message">Повідомлення українською з конкретними значеннями.</param>
    /// <exception cref="ArgumentException">Правило або повідомлення порожні.</exception>
    public DomainRuleException(string rule, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rule);
        Rule = rule;
    }

    /// <summary>
    /// Створює виняток із правилом, повідомленням і первинним винятком,
    /// який зберігається для повного стека викликів.
    /// </summary>
    /// <param name="rule">Ідентифікатор правила, наприклад <c>order.editable</c>.</param>
    /// <param name="message">Повідомлення українською з конкретними значеннями.</param>
    /// <param name="innerException">Первинний виняток, що спричинив порушення.</param>
    /// <exception cref="ArgumentException">Правило або повідомлення порожні.</exception>
    /// <exception cref="ArgumentNullException">Первинний виняток — null.</exception>
    public DomainRuleException(
        string rule,
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rule);
        ArgumentNullException.ThrowIfNull(innerException);
        Rule = rule;
    }

    /// <summary>Ідентифікатор порушеного правила предметної області.</summary>
    public string Rule { get; }
}
