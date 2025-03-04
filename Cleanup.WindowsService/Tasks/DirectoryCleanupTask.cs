using System.Runtime.InteropServices;
using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class DirectoryCleanupTask : BaseCleanupTask
{
    private readonly string _directoryPath;
    private readonly string _description;

    public override string Name => $"{_description} Cleanup";

    public DirectoryCleanupTask(
        ILogger<DirectoryCleanupTask> logger,
        IFileOperations fileOps,
        string directoryPath,
        string description) : base(logger, fileOps)
    {
        _directoryPath = directoryPath;
        _description = description;
    }

    protected override bool ExecuteCore()
    {
        if (!_fileOps.DirectoryExists(_directoryPath))
        {
            _logger.LogInformation("Directory {Path} does not exist, skipping", _directoryPath);
            return true;
        }

        var directoryInfo = _fileOps.GetDirectoryInfo(_directoryPath);

        try
        {
            foreach (var file in directoryInfo.GetFiles())
            {
                try
                {
                    file.Delete();
                    _logger.LogDebug("Deleted file {FileName}", file.FullName);
                }
                catch (Exception ex) when (IsFileLocked(ex) || ex is UnauthorizedAccessException)
                {
                    _logger.LogWarning("Could not delete {FileName}: {Message}", file.FullName, ex.Message);
                }
            }

            foreach (var dir in directoryInfo.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                    _logger.LogDebug("Deleted directory {DirName}", dir.FullName);
                }
                catch (Exception ex) when (IsFileLocked(ex) || ex is UnauthorizedAccessException)
                {
                    _logger.LogWarning("Could not delete {DirName}: {Message}", dir.FullName, ex.Message);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning {Path}", _directoryPath);
            return false;
        }
    }

    private bool IsFileLocked(Exception ex)
    {
        if (ex is not IOException ioException)
            return false;

        int errorCode = Marshal.GetHRForException(ioException) & ((1 << 16) - 1);
        return errorCode == 32 || errorCode == 33;
    }
}