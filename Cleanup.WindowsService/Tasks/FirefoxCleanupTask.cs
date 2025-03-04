using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class FirefoxCleanupTask : BaseCleanupTask
{
    public override string Name => "Firefox Browser Cache Cleanup";
    
    public FirefoxCleanupTask(
        ILogger<FirefoxCleanupTask> logger,
        IFileOperations fileOps) : base(logger, fileOps)
    {
    }
    
    protected override bool ExecuteCore()
    {
        try
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string firefoxProfilesDir = Path.Combine(appData, "Mozilla", "Firefox", "Profiles");
            
            if (!_fileOps.DirectoryExists(firefoxProfilesDir))
            {
                _logger.LogInformation("Firefox profiles directory not found, skipping cache cleanup");
                return true;
            }
            
            string[] profileDirs = _fileOps.GetDirectories(firefoxProfilesDir);
            int totalFilesDeleted = 0;
            
            foreach (var profileDir in profileDirs)
            {
                string cacheDir = Path.Combine(profileDir, "cache2");
                if (_fileOps.DirectoryExists(cacheDir))
                {
                    var cacheFiles = _fileOps.GetFiles(cacheDir, "*.*");
                    foreach (var file in cacheFiles)
                    {
                        _fileOps.DeleteFile(file);
                        totalFilesDeleted++;
                    }
                    
                    foreach (var subDir in _fileOps.GetDirectories(cacheDir))
                    {
                        var subDirFiles = _fileOps.GetFiles(subDir, "*.*");
                        foreach (var file in subDirFiles)
                        {
                            _fileOps.DeleteFile(file);
                            totalFilesDeleted++;
                        }
                    }
                }
                
                string startupCacheDir = Path.Combine(profileDir, "startupCache");
                if (_fileOps.DirectoryExists(startupCacheDir))
                {
                    var startupCacheFiles = _fileOps.GetFiles(startupCacheDir, "*.*");
                    foreach (var file in startupCacheFiles)
                    {
                        _fileOps.DeleteFile(file);
                        totalFilesDeleted++;
                    }
                }
                
                // Clean thumbnails
                string thumbnailsDir = Path.Combine(profileDir, "thumbnails");
                if (_fileOps.DirectoryExists(thumbnailsDir))
                {
                    var thumbnailsFiles = _fileOps.GetFiles(thumbnailsDir, "*.*");
                    foreach (var file in thumbnailsFiles)
                    {
                        _fileOps.DeleteFile(file);
                        totalFilesDeleted++;
                    }
                }
            }
            
            _logger.LogInformation("Firefox browser cache cleaned: {Count} files deleted", totalFilesDeleted);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean Firefox browser cache");
            return false;
        }
    }
}