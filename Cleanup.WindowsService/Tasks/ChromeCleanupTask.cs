using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class ChromeCleanupTask : BaseCleanupTask
{
    public override string Name => "Chrome Browser Cache Cleanup";

    public ChromeCleanupTask(
        ILogger<ChromeCleanupTask> logger,
        IFileOperations fileOps) : base(logger, fileOps)
    {
    }

    protected override bool ExecuteCore()
    {
        try
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string chromeDir = Path.Combine(localAppData, "Google", "Chrome", "User Data");

            if (!_fileOps.DirectoryExists(chromeDir))
            {
                _logger.LogInformation("Chrome directory not found, skipping cache cleanup");
                return true;
            }

            string[] profileDirs = _fileOps.GetDirectories(chromeDir);
            int totalFilesDeleted = 0;

            foreach (var profileDir in profileDirs)
            {
                string profileName = Path.GetFileName(profileDir);
                if (profileName != "Default" && !profileName.StartsWith("Profile"))
                    continue;

                string cacheDir = Path.Combine(profileDir, "Cache");
                if (_fileOps.DirectoryExists(cacheDir))
                {
                    var cacheFiles = _fileOps.GetFiles(cacheDir, "*.*");
                    foreach (var file in cacheFiles)
                    {
                        _fileOps.DeleteFile(file);
                        totalFilesDeleted++;
                    }
                }

                string gpuCacheDir = Path.Combine(profileDir, "GPUCache");
                if (_fileOps.DirectoryExists(gpuCacheDir))
                {
                    var gpuCacheFiles = _fileOps.GetFiles(gpuCacheDir, "*.*");
                    foreach (var file in gpuCacheFiles)
                    {
                        _fileOps.DeleteFile(file);
                        totalFilesDeleted++;
                    }
                }

                string codeCacheDir = Path.Combine(profileDir, "Code Cache");
                if (_fileOps.DirectoryExists(codeCacheDir))
                {
                    var codeCacheFiles = _fileOps.GetFiles(codeCacheDir, "*.*");
                    foreach (var file in codeCacheFiles)
                    {
                        _fileOps.DeleteFile(file);
                        totalFilesDeleted++;
                    }
                }
            }

            _logger.LogInformation("Chrome browser cache cleaned: {Count} files deleted", totalFilesDeleted);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean Chrome browser cache");
            return false;
        }
    }
}