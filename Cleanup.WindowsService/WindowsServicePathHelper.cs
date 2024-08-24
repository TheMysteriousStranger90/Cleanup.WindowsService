using System;
using System.IO;

namespace Cleanup.WindowsService;

public static class WindowsServicePathHelperForLogs
{
    public static string GenerateWindowsServiceFilePath()
    {
        string userProfile = Environment.GetEnvironmentVariable("USERPROFILE") 
                             ?? throw new InvalidOperationException("USERPROFILE environment variable is not set.");
        
        string logDirectoryPath = Path.Combine(userProfile, "Documents", "CleanupWindowsService");

        Directory.CreateDirectory(logDirectoryPath);

        string fileName = $"LoggingResults_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

        return Path.Combine(logDirectoryPath, fileName);
    }
}