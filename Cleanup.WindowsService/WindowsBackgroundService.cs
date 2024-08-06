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

    public WindowsBackgroundService(
        CleanupService cleanupService,
        ILogger<WindowsBackgroundService> logger) =>
        (_cleanupService, _logger) = (cleanupService, logger);

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

                string logFilePath = WindowsServicePathHelperForLogs.GenerateWindowsServiceFilePath();

                using (var writer = new StreamWriter(logFilePath, true))
                {
                    bool success = _cleanupService.RunCleanupTasks();

                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        writer.WriteLine("Cleanup finished at: {0}", DateTimeOffset.Now);
                        _logger.LogInformation("Cleanup finished at: {time}", DateTimeOffset.Now);
                    }

                    var delayTime = success ? TimeSpan.FromHours(1) : TimeSpan.FromHours(2);

                    writer.WriteLine(success ? "Next cleanup scheduled in 24 hours." : "Retry scheduled in 2 hours.");
                    writer.Flush();

                    await Task.Delay(delayTime, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Когда токен остановки отменяется, например, при вызове из services.msc,
                // не следует завершать с ненулевым кодом выхода. Другими словами, это ожидаемое...
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);

                // Завершает этот процесс и возвращает код выхода в операционную систему.
                // Это необходимо, чтобы избежать 'BackgroundServiceExceptionBehavior', который
                // выполняет один из двух сценариев:
                // 1. Когда установлено "Ignore": ничего не делать, ошибки вызывают зомби-сервисы.
                // 2. Когда установлено "StopHost": чисто остановить хост и записать ошибки в лог.
                //
                // Для того чтобы система управления сервисами Windows могла использовать
                // настроенные параметры восстановления, необходимо завершить процесс с ненулевым кодом выхода.
                Environment.Exit(1);
            }
        }
    }
}