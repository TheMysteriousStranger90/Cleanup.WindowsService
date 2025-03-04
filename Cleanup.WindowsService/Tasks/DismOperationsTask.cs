using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class DismOperationsTask : BaseCleanupTask
{
    private readonly IProcessRunner _processRunner;

    public override string Name => "DISM Cleanup Operations";

    public DismOperationsTask(
        ILogger<DismOperationsTask> logger,
        IFileOperations fileOps,
        IProcessRunner processRunner) : base(logger, fileOps)
    {
        _processRunner = processRunner;
    }

    protected override bool ExecuteCore()
    {
        _logger.LogInformation("Running DISM to clean old service pack files...");
        int exitCode = _processRunner.RunProcessWithExitCode("dism.exe", "/online /cleanup-Image /spsuperseded");

        if (exitCode == 0)
        {
            _logger.LogInformation("DISM operation completed successfully");
            return true;
        }
        else
        {
            _logger.LogWarning("DISM operation failed with exit code: {ExitCode}", exitCode);
            return false;
        }
    }
}