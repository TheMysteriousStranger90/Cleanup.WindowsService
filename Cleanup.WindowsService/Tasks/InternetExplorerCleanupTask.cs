using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class InternetExplorerCleanupTask : BaseCleanupTask
{
    private readonly IProcessRunner _processRunner;
    
    public override string Name => "Internet Explorer Cache Cleanup";
    
    public InternetExplorerCleanupTask(
        ILogger<InternetExplorerCleanupTask> logger,
        IFileOperations fileOps,
        IProcessRunner processRunner) : base(logger, fileOps)
    {
        _processRunner = processRunner;
    }
    
    protected override bool ExecuteCore()
    {
        try
        {
            _logger.LogInformation("Erasing Internet Explorer temporary data...");
            
            bool result = _processRunner.RunProcess("rundll32.exe", "inetcpl.cpl,ClearMyTracksByProcess 4351");
            
            if (result)
            {
                _logger.LogInformation("Internet Explorer temporary data erased successfully");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean Internet Explorer cache");
            return false;
        }
    }
}