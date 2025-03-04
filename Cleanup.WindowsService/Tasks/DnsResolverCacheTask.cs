using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class DnsResolverCacheTask : BaseCleanupTask
{
    private readonly IProcessRunner _processRunner;

    public override string Name => "DNS Resolver Cache Cleanup";

    public DnsResolverCacheTask(
        ILogger<DnsResolverCacheTask> logger,
        IFileOperations fileOps,
        IProcessRunner processRunner) : base(logger, fileOps)
    {
        _processRunner = processRunner;
    }

    protected override bool ExecuteCore()
    {
        _logger.LogInformation("Resetting DNS resolver cache...");
        bool result = _processRunner.RunProcess("ipconfig.exe", "/flushdns");

        if (result)
        {
            _logger.LogInformation("DNS resolver cache reset successfully");
        }

        return result;
    }
}