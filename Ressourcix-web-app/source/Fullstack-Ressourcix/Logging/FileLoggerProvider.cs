namespace FullstackRessourcix;

using Microsoft.Extensions.Logging;

// Ergänzt die bereits im ganzen Backend vorhandenen ILogger<T>-Aufrufe (EmployeeStore, AuthStore,
// RequestsStore, GlobalExceptionHandler, ASP.NET Core selbst, ...) um einen zweiten Sink: dieselben
// Log-Einträge landen zusätzlich zur Konsole in einer Datei, ohne dass an den Log-Aufrufen selbst
// etwas geändert werden muss. Respektiert dieselbe Logging:LogLevel-Konfiguration wie jeder andere
// Provider (die Filterung passiert oberhalb, in LoggerFactory/LoggerFilterOptions).
public sealed class FileLoggerProvider : ILoggerProvider
{
  private readonly StreamWriter _writer;
  private readonly Lock _writeLock = new();

  public FileLoggerProvider(string filePath)
  {
    var directory = Path.GetDirectoryName(filePath);
    if (!string.IsNullOrEmpty(directory))
    {
      Directory.CreateDirectory(directory);
    }

    _writer = new StreamWriter(
      new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read)
    )
    {
      AutoFlush = true,
    };
  }

  public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, _writer, _writeLock);

  public void Dispose() => _writer.Dispose();
}

internal sealed class FileLogger(string categoryName, TextWriter writer, Lock writeLock) : ILogger
{
  public IDisposable? BeginScope<TState>(TState state)
    where TState : notnull => null;

  public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

  public void Log<TState>(
    LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter
  )
  {
    if (!IsEnabled(logLevel))
      return;

    var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {categoryName}: {formatter(state, exception)}";
    if (exception is not null)
    {
      line += Environment.NewLine + exception;
    }

    lock (writeLock)
    {
      writer.WriteLine(line);
    }
  }
}
