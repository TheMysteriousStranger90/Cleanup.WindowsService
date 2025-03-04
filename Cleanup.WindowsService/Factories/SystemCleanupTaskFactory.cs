using Cleanup.WindowsService.Interfaces;
using Cleanup.WindowsService.Tasks;

namespace Cleanup.WindowsService.Factories;

public class SystemCleanupTaskFactory : ICleanupTaskFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileOperations _fileOps;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IProcessRunner _processRunner;

    public SystemCleanupTaskFactory(
        IServiceProvider serviceProvider,
        IFileOperations fileOps,
        ILoggerFactory loggerFactory,
        IProcessRunner processRunner)
    {
        _serviceProvider = serviceProvider;
        _fileOps = fileOps;
        _loggerFactory = loggerFactory;
        _processRunner = processRunner;
    }

    public IEnumerable<ICleanupTask> CreateTasks()
    {
        // Get common paths
        string windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string userProfile = Environment.GetEnvironmentVariable("USERPROFILE") ?? "";
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        // Core system tasks
        yield return _serviceProvider.GetRequiredService<RecycleBinCleanupTask>();
        yield return _serviceProvider.GetRequiredService<EventLogCleanupTask>();
        yield return _serviceProvider.GetRequiredService<DnsResolverCacheTask>();
        yield return _serviceProvider.GetRequiredService<DismOperationsTask>();
        yield return _serviceProvider.GetRequiredService<SystemRestorePointsTask>();
        yield return _serviceProvider.GetRequiredService<WindowsUpdateCacheTask>();
        yield return _serviceProvider.GetRequiredService<SystemFileCheckerTask>();
        
        yield return new DirectoryCleanupTask(
            _loggerFactory.CreateLogger<DirectoryCleanupTask>(), 
            _fileOps,
            Path.GetTempPath(),
            "User Temp");
            
        yield return new DirectoryCleanupTask(
            _loggerFactory.CreateLogger<DirectoryCleanupTask>(), 
            _fileOps,
            Path.Combine(windowsPath, "Temp"),
            "Windows Temp");
            
        yield return new DirectoryCleanupTask(
            _loggerFactory.CreateLogger<DirectoryCleanupTask>(), 
            _fileOps,
            Path.Combine(windowsPath, "Prefetch"),
            "Prefetch");

        yield return new DirectoryCleanupTask(
            _loggerFactory.CreateLogger<DirectoryCleanupTask>(), 
            _fileOps,
            Path.Combine(userProfile, "Downloads"),
            "Downloads");

        string[] logPaths = {
            Path.Combine(windowsPath, "Logs"),
            Path.Combine(windowsPath, "Panther"),
            Path.Combine(windowsPath, "SoftwareDistribution", "Download"),
            Path.Combine(programData, "Microsoft", "Windows", "WER", "Temp")
        };
        
        foreach (var path in logPaths)
        {
            yield return new DirectoryCleanupTask(
                _loggerFactory.CreateLogger<DirectoryCleanupTask>(),
                _fileOps,
                path,
                $"Log files in {Path.GetFileName(path)}");
        }
        
        yield return new DirectoryCleanupTask(
            _loggerFactory.CreateLogger<DirectoryCleanupTask>(),
            _fileOps,
            Path.Combine(userProfile, "AppData", "Roaming", "Microsoft", "Windows", "Cookies"),
            "Cookies");
        
        yield return new DirectoryCleanupTask(
            _loggerFactory.CreateLogger<DirectoryCleanupTask>(),
            _fileOps,
            Path.Combine(userProfile, "AppData", "Local", "Microsoft", "Windows", "History"),
            "History");

        string[] driverPaths = {
            Path.Combine(userProfile, "AMD"),
            Path.Combine(userProfile, "NVIDIA"),
            Path.Combine(userProfile, "INTEL")
        };
        
        foreach (var path in driverPaths)
        {
            yield return new DirectoryCleanupTask(
                _loggerFactory.CreateLogger<DirectoryCleanupTask>(),
                _fileOps,
                path,
                $"Driver files in {Path.GetFileName(path)}");
        }
        
        yield return new ThumbnailCacheCleanupTask(
            _loggerFactory.CreateLogger<ThumbnailCacheCleanupTask>(),
            _fileOps);
        
        yield return new MemoryDumpCleanupTask(
            _loggerFactory.CreateLogger<MemoryDumpCleanupTask>(),
            _fileOps);
        
        yield return new PatternCleanupTask(
            _loggerFactory.CreateLogger<PatternCleanupTask>(),
            _fileOps,
            new[] { windowsPath, userProfile },
            new[] { "*.old", "*.bak", "*.tmp" },
            "Old files");
            
        yield return new PatternCleanupTask(
            _loggerFactory.CreateLogger<PatternCleanupTask>(),
            _fileOps,
            new[] { windowsPath, userProfile },
            new[] { "*.trace" },
            "Trace files");
    }
}