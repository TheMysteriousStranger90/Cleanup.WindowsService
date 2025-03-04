using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class PatternCleanupTask : BaseCleanupTask
{
    private readonly string[] _directories;
    private readonly string[] _patterns;
    private readonly string _description;

    public override string Name => $"{_description} Cleanup";

    public PatternCleanupTask(
        ILogger logger,
        IFileOperations fileOps,
        string[] directories,
        string[] patterns,
        string description) : base(logger, fileOps)
    {
        _directories = directories;
        _patterns = patterns;
        _description = description;
    }

    protected override bool ExecuteCore()
    {
        bool success = true;

        foreach (var directory in _directories)
        {
            if (!_fileOps.DirectoryExists(directory))
                continue;

            foreach (var pattern in _patterns)
            {
                try
                {
                    var files = _fileOps.GetFiles(directory, pattern);
                    foreach (var file in files)
                    {
                        if (!_fileOps.DeleteFile(file))
                            success = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting files with pattern {Pattern} in {Directory}", pattern,
                        directory);
                    success = false;
                }
            }
        }

        return success;
    }
}