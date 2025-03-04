using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class SystemFileCheckerTask : BaseCleanupTask
{
    private readonly IProcessRunner _processRunner;

    public override string Name => "System File Checker";

    public SystemFileCheckerTask(
        ILogger<SystemFileCheckerTask> logger,
        IFileOperations fileOps,
        IProcessRunner processRunner) : base(logger, fileOps)
    {
        _processRunner = processRunner;
    }

    protected override bool ExecuteCore()
    {
        _logger.LogInformation("Running system file checker...");
        bool result = _processRunner.RunProcess("sfc.exe", "/scannow");

        if (result)
        {
            _logger.LogInformation("System file checker completed successfully");
        }

        return result;
    }
}