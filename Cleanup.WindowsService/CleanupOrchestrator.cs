using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService;

public class CleanupOrchestrator
{
    private readonly ILogger<CleanupOrchestrator> _logger;
    private readonly IEnumerable<ICleanupTaskFactory> _taskFactories;

    public CleanupOrchestrator(
        ILogger<CleanupOrchestrator> logger,
        IEnumerable<ICleanupTaskFactory> taskFactories)
    {
        _logger = logger;
        _taskFactories = taskFactories;
    }

    public async Task RunCleanupTasksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting cleanup tasks");
            
            var allTasks = _taskFactories
                .SelectMany(factory => factory.CreateTasks())
                .ToList();
                
            _logger.LogInformation("Found {Count} cleanup tasks to execute", allTasks.Count);
            
            int successCount = 0;
            int failedCount = 0;
            
            foreach (var task in allTasks)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Cleanup process was cancelled");
                    break;
                }
                
                _logger.LogInformation("Executing task: {TaskName}", task.Name);
                bool success = task.Execute();
                
                if (success)
                    successCount++;
                else
                    failedCount++;
            }
            
            _logger.LogInformation("Cleanup completed. Success: {SuccessCount}, Failed: {FailedCount}", 
                successCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running cleanup tasks");
        }
    }
}