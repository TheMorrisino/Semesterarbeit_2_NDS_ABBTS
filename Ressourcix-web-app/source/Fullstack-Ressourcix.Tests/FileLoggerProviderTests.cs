using FullstackRessourcix;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Fullstack_Ressourcix.Tests;

public class FileLoggerProviderTests
{
    private static string CreateTempLogPath () =>
        Path.Combine(Path.GetTempPath(), $"ressourcix-logtest-{Guid.NewGuid()}", "test.log");

    [Fact]
    public void Log_SchreibtNachrichtMitLevelUndKategorieInDieDatei ()
    {
        var path = CreateTempLogPath ();
        try
        {
            using (var provider = new FileLoggerProvider(path))
            {
                var logger = provider.CreateLogger("MeineKategorie");
                logger.LogInformation("Testnachricht {Value}", 42);
            }

            var content = File.ReadAllText(path);

            Assert.Contains("Information", content);
            Assert.Contains("MeineKategorie", content);
            Assert.Contains("Testnachricht 42", content);
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public void Log_SchreibtExceptionDetailsMitInDieDatei ()
    {
        var path = CreateTempLogPath ();
        try
        {
            using (var provider = new FileLoggerProvider(path))
            {
                var logger = provider.CreateLogger("Fehlerkategorie");
                logger.LogError(new InvalidOperationException("Kaputt"), "Etwas ist fehlgeschlagen");
            }

            var content = File.ReadAllText(path);

            Assert.Contains("Etwas ist fehlgeschlagen", content);
            Assert.Contains("InvalidOperationException", content);
            Assert.Contains("Kaputt", content);
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public void CreateLogger_LegtVerzeichnisAnFallsEsNochNichtExistiert ()
    {
        var path = CreateTempLogPath ();
        var directory = Path.GetDirectoryName(path)!;
        Assert.False(Directory.Exists(directory));

        try
        {
            using var provider = new FileLoggerProvider(path);

            Assert.True(Directory.Exists(directory));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public void MehrereLoggerAusDemselbenProvider_SchreibenInDieselbeDatei ()
    {
        var path = CreateTempLogPath ();
        try
        {
            using (var provider = new FileLoggerProvider(path))
            {
                provider.CreateLogger("Kategorie1").LogInformation("Erste Nachricht");
                provider.CreateLogger("Kategorie2").LogWarning("Zweite Nachricht");
            }

            var content = File.ReadAllText(path);

            Assert.Contains("Erste Nachricht", content);
            Assert.Contains("Zweite Nachricht", content);
        }
        finally
        {
            CleanUp(path);
        }
    }

    private static void CleanUp (string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (directory is not null && Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
