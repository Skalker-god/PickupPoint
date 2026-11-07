namespace PickupPoint.Core.Audit;

/// <summary>
/// Журнал аудиту у файлі: дописує операцію, документ і час. Володіє
/// <see cref="StreamWriter"/> і звільняє його методом <see cref="Dispose"/>.
/// </summary>
public sealed class FileAuditLog : IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    /// <summary>Відкриває файл журналу в режимі дописування.</summary>
    /// <param name="path">Непорожній шлях до файлу журналу.</param>
    /// <exception cref="ArgumentException">Шлях порожній.</exception>
    /// <exception cref="IOException">Файл не вдалося відкрити.</exception>
    public FileAuditLog(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _writer = new StreamWriter(path, append: true);
    }

    /// <summary>Дописує рядок аудиту та негайно скидає його на диск.</summary>
    /// <param name="operation">Назва операції, наприклад <c>add-line</c>.</param>
    /// <param name="documentId">Ідентифікатор документа.</param>
    /// <exception cref="ArgumentException">Назва операції порожня.</exception>
    /// <exception cref="ObjectDisposedException">Журнал уже закрито.</exception>
    public void Write(string operation, int documentId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        _writer.WriteLine(
            $"{DateTimeOffset.Now:O};{operation};{documentId}");
        _writer.Flush();
    }

    /// <summary>
    /// Закриває файл журналу; повторний виклик нічого не робить.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _writer.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
