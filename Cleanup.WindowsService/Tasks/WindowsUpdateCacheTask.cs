using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class WindowsUpdateCacheTask : BaseCleanupTask
{
    private readonly IProcessRunner _processRunner;

    public override string Name => "Windows Update Cache Cleanup";

    public WindowsUpdateCacheTask(
        ILogger<WindowsUpdateCacheTask> logger,
        IFileOperations fileOps,
        IProcessRunner processRunner) : base(logger, fileOps)
    {
        _processRunner = processRunner;
    }

    protected override bool ExecuteCore()
    {
        _logger.LogInformation("Cleaning Windows Update cache...");
        bool result = _processRunner.RunProcess("cleanmgr.exe", "/sagerun:1");

        if (result)
        {
            _logger.LogInformation("Windows Update cache cleaned successfully");
        }

        return result;
    }
}