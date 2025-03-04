using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class ThumbnailCacheCleanupTask : BaseCleanupTask
{
    public override string Name => "Thumbnail Cache Cleanup";
    
    public ThumbnailCacheCleanupTask(
        ILogger<ThumbnailCacheCleanupTask> logger,
        IFileOperations fileOps) : base(logger, fileOps)
    {
    }
    
    protected override bool ExecuteCore()
    {
        try
        {
            string userProfile = Environment.GetEnvironmentVariable("USERPROFILE")
                                 ?? throw new InvalidOperationException("USERPROFILE environment variable is not set");
            
            string thumbnailCachePath = Path.Combine(userProfile, "AppData", "Local", "Microsoft", "Windows", "Explorer");
            
            if (!_fileOps.DirectoryExists(thumbnailCachePath))
            {
                _logger.LogInformation("Thumbnail cache directory not found");
                return true;
            }
            
            var directoryInfo = _fileOps.GetDirectoryInfo(thumbnailCachePath);
            foreach (var file in directoryInfo.GetFiles("thumbcache_*.db"))
            {
                _fileOps.DeleteFile(file.FullName);
                _logger.LogDebug("Deleted thumbnail cache file: {FileName}", file.Name);
            }
            
            string iconCachePath = Path.Combine(thumbnailCachePath, "IconCache.db");
            if (_fileOps.FileExists(iconCachePath))
            {
                _fileOps.DeleteFile(iconCachePath);
                _logger.LogDebug("Deleted icon cache file");
            }
            
            _logger.LogInformation("Thumbnail cache cleaned successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean thumbnail cache");
            return false;
        }
    }
}