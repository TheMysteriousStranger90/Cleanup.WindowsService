using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public abstract class BaseCleanupTask : ICleanupTask
{
    protected readonly ILogger _logger;
    protected readonly IFileOperations _fileOps;

    public abstract string Name { get; }

    protected BaseCleanupTask(ILogger logger, IFileOperations fileOps)
    {
        _logger = logger;
        _fileOps = fileOps;
    }

    public bool Execute()
    {
        try
        {
            _logger.LogInformation("Starting {TaskName} task", Name);
            var result = ExecuteCore();
            _logger.LogInformation("{TaskName} completed successfully", Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing {TaskName}", Name);
            return false;
        }
    }

    protected abstract bool ExecuteCore();
}