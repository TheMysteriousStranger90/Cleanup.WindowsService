using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Cleanup.WindowsService;

public sealed class CleanupService
{
    private readonly ILogger<CleanupService> _logger;

    public CleanupService(ILogger<CleanupService> logger)
    {
        _logger = logger;
    }

    [DllImport("Shell32.dll")]
    static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);

    public void EmptyRecycleBin()
    {
        try
        {
            SHEmptyRecycleBin(IntPtr.Zero, null, 0);
            _logger.LogInformation("Recycle bin emptied successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error emptying recycle bin");
        }
    }

    public void CleanTempFolder()
    {
        CleanDirectory(Path.GetTempPath(), "Temp folder");
    }
    
    public void CleanDownloadsFolder()
    {
        string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        CleanDirectory(downloadsPath, "Downloads");
    }

    public void CleanPrefetchFolder()
    {
        string prefetchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
        CleanDirectory(prefetchPath, "Prefetch");
    }

    public void CleanWindowsTempFolder()
    {
        string windowsTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
        CleanDirectory(windowsTempPath, "Temp");
    }

    public void CleanLogFiles()
    {
        string[] logPaths =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Logs"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Panther"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Windows", "WER", "Temp")
        };

        foreach (var logPath in logPaths)
        {
            if (Directory.Exists(logPath))
            {
                CleanDirectory(logPath, "Log files");
            }
        }
    }

    public void CleanEventLogs()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "wevtutil.exe",
                Arguments = "cl Application",
                CreateNoWindow = true,
                UseShellExecute = false
            })?.WaitForExit();

            Process.Start(new ProcessStartInfo
            {
                FileName = "wevtutil.exe",
                Arguments = "cl System",
                CreateNoWindow = true,
                UseShellExecute = false
            })?.WaitForExit();

            _logger.LogInformation("Event logs cleaned successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning event logs");
        }
    }

    public void CleanOldFiles()
    {
        string[] oldFilePatterns = { "*.old", "*.bak", "*.tmp" };
        string[] directoriesToClean =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        foreach (var dir in directoriesToClean)
        {
            foreach (var pattern in oldFilePatterns)
            {
                DeleteFilesByPattern(dir, pattern);
            }
        }
    }

    public void CleanTraceFiles()
    {
        string[] traceFilePatterns = { "*.trace" };
        string[] directoriesToClean =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        foreach (var dir in directoriesToClean)
        {
            foreach (var pattern in traceFilePatterns)
            {
                DeleteFilesByPattern(dir, pattern);
            }
        }
    }

    public void CleanHistory()
    {
        string historyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.History));
        if (Directory.Exists(historyPath))
        {
            CleanDirectory(historyPath, "History");
        }
    }

    public void CleanCookies()
    {
        string cookiesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Cookies));
        if (Directory.Exists(cookiesPath))
        {
            CleanDirectory(cookiesPath, "Cookies");
        }
    }

    public void CleanRemnantDriverFiles()
    {
        string[] driverPaths = { "C:\\AMD", "C:\\NVIDIA", "C:\\INTEL" };

        foreach (var driverPath in driverPaths)
        {
            if (Directory.Exists(driverPath))
            {
                CleanDirectory(driverPath, "Remnant driver files");
            }
        }
    }

    public void ResetDnsResolverCache()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ipconfig.exe",
                Arguments = "/flushdns",
                CreateNoWindow = true,
                UseShellExecute = false
            })?.WaitForExit();

            _logger.LogInformation("DNS resolver cache reset successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting DNS resolver cache");
        }
    }

    public void RunSystemFileChecker()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "sfc.exe",
                Arguments = "/scannow",
                CreateNoWindow = true,
                UseShellExecute = false
            })?.WaitForExit();

            _logger.LogInformation("System file checker completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running system file checker");
        }
    }

    private void CleanDirectory(string path, string folderName)
    {
        try
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                _logger.LogInformation($"{folderName} cleaned successfully.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error cleaning {folderName}");
        }
    }

    private void DeleteFilesByPattern(string directory, string pattern)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                        _logger.LogInformation($"File {file} deleted successfully.");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        _logger.LogError(ex, $"Access to the file '{file}' is denied.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error deleting file '{file}'");
                    }
                }

                _logger.LogInformation($"Files with pattern {pattern} deleted successfully in {directory}.");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, $"Access to the directory '{directory}' is denied.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting files with pattern {pattern} in {directory}");
        }
    }
}