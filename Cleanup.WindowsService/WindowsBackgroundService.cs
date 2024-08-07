namespace Cleanup.WindowsService;

public sealed class WindowsBackgroundService : BackgroundService
{
    private readonly CleanupService _cleanupService;
    private readonly ILogger<WindowsBackgroundService> _logger;
    
    public WindowsBackgroundService(CleanupService cleanupService, ILogger<WindowsBackgroundService> logger)
    {
        _cleanupService = cleanupService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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

                var delayTime = success ? TimeSpan.FromHours(12) : TimeSpan.FromHours(6);
                
                _logger.LogInformation(success ? "Next cleanup scheduled in 12 hours." : "Retry scheduled in 6 hours.");

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