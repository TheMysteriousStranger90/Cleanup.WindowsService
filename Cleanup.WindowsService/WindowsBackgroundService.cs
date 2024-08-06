using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cleanup.WindowsService;

public sealed class WindowsBackgroundService : BackgroundService
{
    private readonly CleanupService _cleanupService;
    private readonly ILogger<WindowsBackgroundService> _logger;
    private readonly ILogger _fileLogger;
    private readonly string _logFilePath = WindowsServicePathHelperForLogs.GenerateWindowsServiceFilePath();

    public WindowsBackgroundService(
        CleanupService cleanupService,
        ILogger<WindowsBackgroundService> logger)
    {
        _cleanupService = cleanupService;
        _logger = logger;

        var fileLoggerProvider = new FileLoggerProvider(_logFilePath);
        _fileLogger = fileLoggerProvider.CreateLogger(nameof(WindowsBackgroundService));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //await Task.Delay(TimeSpan.FromHours(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Cleanup started at: {time}", DateTimeOffset.Now);
                }

                bool success = _cleanupService.RunCleanupTasks();

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Cleanup finished at: {time}", DateTimeOffset.Now);
                }

                var delayTime = success ? TimeSpan.FromDays(1) : TimeSpan.FromHours(2);

                _logger.LogInformation(success ? "Next cleanup scheduled in 24 hours." : "Retry scheduled in 2 hours.");

                await Task.Delay(delayTime, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);

                Environment.Exit(1);
            }
        }
    }
}