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
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Cleanup started at: {time}", DateTimeOffset.Now);
                }

                _cleanupService.EmptyRecycleBin();
                _cleanupService.CleanTempFolder();
                _cleanupService.CleanDownloadsFolder();
                _cleanupService.CleanPrefetchFolder();
                _cleanupService.CleanWindowsTempFolder();
                _cleanupService.CleanLogFiles();
                _cleanupService.CleanEventLogs();
                _cleanupService.CleanOldFiles();
                _cleanupService.CleanTraceFiles();
                _cleanupService.CleanHistory();
                _cleanupService.CleanCookies();
                _cleanupService.CleanRemnantDriverFiles();
                _cleanupService.ResetDnsResolverCache();
                _cleanupService.RunSystemFileChecker();

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Cleanup finished at: {time}", DateTimeOffset.Now);
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // When the stopping token is canceled, for example, a call made from services.msc,
            // we shouldn't exit with a non-zero exit code. In other words, this is expected...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);

            // Terminates this process and returns an exit code to the operating system.
            // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
            // performs one of two scenarios:
            // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
            // 2. When set to "StopHost": will cleanly stop the host, and log errors.
            //
            // In order for the Windows Service Management system to leverage configured
            // recovery options, we need to terminate the process with a non-zero exit code.
            Environment.Exit(1);
        }
    }
}