using PickupPoint.Core.Audit;
using Xunit;

namespace PickupPoint.Tests;

public sealed class FileAuditLogTests
{
    [Fact]
    public void Write_AppendsOperationDocumentAndTime()
    {
        string path = TempPath();
        try
        {
            using (FileAuditLog log = new(path))
            {
                log.Write("add-line", 1);
                log.Write("cancel", 1);
            }

            string[] lines = File.ReadAllLines(path);
            Assert.Equal(2, lines.Length);
            Assert.Contains(";add-line;1", lines[0]);
            Assert.Contains(";cancel;1", lines[1]);
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public void Write_AfterDispose_ThrowsObjectDisposed()
    {
        string path = TempPath();
        try
        {
            FileAuditLog log = new(path);
            log.Dispose();

            Assert.Throws<ObjectDisposedException>(
                () => log.Write("report", 1));
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public void AfterExceptionInsideUsing_FileHandleIsReleased()
    {
        string path = TempPath();
        try
        {
            Action action = () =>
            {
                using FileAuditLog log = new(path);
                log.Write("before-error", 1);
                throw new InvalidOperationException("Збій під час операції");
            };
            Assert.Throws<InvalidOperationException>(action);

            File.Delete(path);
            Assert.False(File.Exists(path));
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        string path = TempPath();
        try
        {
            FileAuditLog log = new(path);
            log.Dispose();
            log.Dispose();
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    private static string TempPath() =>
        Path.Combine(
            Path.GetTempPath(),
            $"pickup-audit-{Guid.NewGuid():N}.log");

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
