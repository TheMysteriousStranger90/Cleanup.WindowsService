namespace Cleanup.WindowsService.Interfaces;

public interface IFileOperations
{
    bool DeleteFile(string path);
    bool DeleteDirectory(string path, bool recursive);
    bool FileExists(string path);
    bool DirectoryExists(string path);
    string[] GetFiles(string directory, string searchPattern);
    string[] GetDirectories(string directory);
    DirectoryInfo GetDirectoryInfo(string path);
}