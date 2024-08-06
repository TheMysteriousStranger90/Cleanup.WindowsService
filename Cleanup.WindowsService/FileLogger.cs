namespace Cleanup.WindowsService;

public class FileLogger : ILogger
{
    private readonly string _filePath;

    public FileLogger(string filePath)
    {
        _filePath = filePath;
    }

    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var logRecord = $"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")} [{logLevel}] {formatter(state, exception)}";

        if (exception != null)
        {
            logRecord += $"\nException: {exception}";
        }

        try
        {
            lock (_filePath)
            {
                File.AppendAllText(_filePath, logRecord + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }
}