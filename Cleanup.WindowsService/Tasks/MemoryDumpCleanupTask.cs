using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class MemoryDumpCleanupTask : BaseCleanupTask
{
    public override string Name => "Memory Dump Files Cleanup";
    
    public MemoryDumpCleanupTask(
        ILogger<MemoryDumpCleanupTask> logger,
        IFileOperations fileOps) : base(logger, fileOps)
    {
    }
    
    protected override bool ExecuteCore()
    {
        try
        {
            string windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
            string memoryDumpPath = Path.Combine(windowsPath, "MEMORY.DMP");
            if (_fileOps.FileExists(memoryDumpPath))
            {
                _fileOps.DeleteFile(memoryDumpPath);
                _logger.LogInformation("Full memory dump file deleted");
            }
            
            string minidumpPath = Path.Combine(windowsPath, "Minidump");
            if (_fileOps.DirectoryExists(minidumpPath))
            {
                var minidumpFiles = _fileOps.GetFiles(minidumpPath, "*.*");
                foreach (var file in minidumpFiles)
                {
                    _fileOps.DeleteFile(file);
                }
                _logger.LogInformation("Cleaned {Count} memory minidump files", minidumpFiles.Length);
            }
            
            string crashDumpsPath = Path.Combine(localAppData, "CrashDumps");
            if (_fileOps.DirectoryExists(crashDumpsPath))
            {
                var crashDumpFiles = _fileOps.GetFiles(crashDumpsPath, "*.*");
                foreach (var file in crashDumpFiles)
                {
                    _fileOps.DeleteFile(file);
                }
                _logger.LogInformation("Cleaned {Count} application crash dump files", crashDumpFiles.Length);
            }
            
            string[] memoryFilePatterns = { "*.hdmp", "*.mdmp", "*.dmp" };
            int deletedCount = 0;
            
            foreach (var pattern in memoryFilePatterns)
            {
                var dumpFiles = _fileOps.GetFiles(windowsPath, pattern);
                foreach (var file in dumpFiles)
                {
                    if (_fileOps.DeleteFile(file))
                        deletedCount++;
                }
            }
            
            _logger.LogInformation("Deleted {Count} additional memory dump files", deletedCount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean memory dump files");
            return false;
        }
    }
}