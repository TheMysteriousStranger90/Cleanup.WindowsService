using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class EventLogCleanupTask : BaseCleanupTask
{
    private readonly IProcessRunner _processRunner;

    public override string Name => "Event Log Cleanup";

    public EventLogCleanupTask(
        ILogger<EventLogCleanupTask> logger,
        IFileOperations fileOps,
        IProcessRunner processRunner) : base(logger, fileOps)
    {
        _processRunner = processRunner;
    }

    protected override bool ExecuteCore()
    {
        bool applicationLogResult = _processRunner.RunProcess("wevtutil.exe", "cl Application");
        bool systemLogResult = _processRunner.RunProcess("wevtutil.exe", "cl System");

        return applicationLogResult && systemLogResult;
    }
}