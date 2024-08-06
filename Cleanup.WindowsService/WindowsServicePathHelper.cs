using System;
using System.IO;

namespace Cleanup.WindowsService;

public static class WindowsServicePathHelperForLogs
{
    public static string GenerateWindowsServiceFilePath()
    {
        var logDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CleanupWindowsService");
        
        Directory.CreateDirectory(logDirectoryPath);
        
        string fileName = $"LoggingResults_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        return Path.Combine(logDirectoryPath, fileName);
    }
}