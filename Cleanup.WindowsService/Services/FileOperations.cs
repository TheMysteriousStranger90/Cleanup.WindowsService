using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Services;

public class FileOperations : IFileOperations
{
    private readonly ILogger<FileOperations> _logger;

    public FileOperations(ILogger<FileOperations> logger)
    {
        _logger = logger;
    }

    public bool DeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {Path}", path);
            return false;
        }
    }

    public bool DeleteDirectory(string path, bool recursive)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting directory {Path}", path);
            return false;
        }
    }

    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public string[] GetFiles(string directory, string searchPattern)
    {
        try
        {
            var enumerationOptions = new EnumerationOptions
            {
                IgnoreInaccessible = true,
            };
            return Directory.GetFiles(directory, searchPattern, enumerationOptions);
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }
    }

    public string[] GetDirectories(string directory)
    {
        try
        {
            return Directory.GetDirectories(directory);
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }
    }

    public DirectoryInfo GetDirectoryInfo(string path) => new DirectoryInfo(path);
}