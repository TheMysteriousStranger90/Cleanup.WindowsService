using Cleanup.WindowsService.Interfaces;
using Cleanup.WindowsService.Tasks;

namespace Cleanup.WindowsService.Factories;

public class BrowserCleanupTaskFactory : ICleanupTaskFactory
{
    private readonly IServiceProvider _serviceProvider;

    public BrowserCleanupTaskFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<ICleanupTask> CreateTasks()
    {
        yield return _serviceProvider.GetRequiredService<ChromeCleanupTask>();
        yield return _serviceProvider.GetRequiredService<FirefoxCleanupTask>();
        yield return _serviceProvider.GetRequiredService<InternetExplorerCleanupTask>();
    }
}