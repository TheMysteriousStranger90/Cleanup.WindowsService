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
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service is starting...");

        await base.StartAsync(cancellationToken);
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service is stopping...");

        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Service execution started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Cleanup started at: {time}", DateTimeOffset.Now);

                _cleanupService.RunCleanupTasks();

                _logger.LogInformation("Cleanup finished at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("Next cleanup scheduled in 12 hours.");

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Service is stopping due to cancellation.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);

                _logger.LogInformation("Retry scheduled in 1 hour due to an error.");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}