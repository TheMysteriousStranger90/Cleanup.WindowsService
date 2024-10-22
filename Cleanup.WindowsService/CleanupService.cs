﻿using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Cleanup.WindowsService;

public sealed class CleanupService
{
    private readonly ILogger<CleanupService> _logger;

    public CleanupService(ILogger<CleanupService> logger)
    {
        _logger = logger;
    }

    public void RunCleanupTasks()
    {
        try
        {
            EmptyRecycleBin();
            CleanDownloadsFolder();
            CleanCookies();
            CleanRemnantDriverFiles();
            ResetDnsResolverCache();
            CleanOldFiles();
            CleanTraceFiles();
            CleanHistory();
            CleanTempFolder();
            CleanPrefetchFolder();
            CleanWindowsTempFolder();
            CleanLogFiles();
            CleanEventLogs();
            EraseInternetExplorerHistory();
            RunDISMOperations();

            _logger.LogInformation("All cleanup tasks completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running cleanup tasks");
        }
    }

    [DllImport("Shell32.dll")]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);

    private const uint SHERB_NOCONFIRMATION = 0x00000001;
    private const uint SHERB_NOPROGRESSUI = 0x00000002;
    private const uint SHERB_NOSOUND = 0x00000004;

    private void EmptyRecycleBin()
    {
        try
        {
            uint flags = SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND;
            SHEmptyRecycleBin(IntPtr.Zero, null, flags);
            _logger.LogInformation("Recycle bin emptied successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error emptying recycle bin");
        }
    }

    private void CleanDownloadsFolder()
    {
        string userProfile = Environment.GetEnvironmentVariable("USERPROFILE")
                             ?? throw new InvalidOperationException("USERPROFILE environment variable is not set.");

        string downloadsPath = Path.Combine(userProfile, "Downloads");
        CleanDirectory(downloadsPath, "Downloads");
    }

    private void CleanOldFiles()
    {
        string userProfile = Environment.GetEnvironmentVariable("USERPROFILE")
                             ?? throw new InvalidOperationException("USERPROFILE environment variable is not set.");

        string[] oldFilePatterns = { "*.old", "*.bak", "*.tmp" };
        string[] directoriesToClean =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            userProfile
        };

        foreach (var dir in directoriesToClean)
        {
            foreach (var pattern in oldFilePatterns)
            {
                DeleteFilesByPattern(dir, pattern);
            }
        }
    }

    private void CleanTraceFiles()
    {
        string userProfile = Environment.GetEnvironmentVariable("USERPROFILE")
                             ?? throw new InvalidOperationException("USERPROFILE environment variable is not set.");

        string[] traceFilePatterns = { "*.trace" };
        string[] directoriesToClean =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            userProfile
        };

        foreach (var dir in directoriesToClean)
        {
            foreach (var pattern in traceFilePatterns)
            {
                DeleteFilesByPattern(dir, pattern);
            }
        }
    }

    private void CleanCookies()
    {
        string userProfile = Environment.GetEnvironmentVariable("USERPROFILE")
                             ?? throw new InvalidOperationException("USERPROFILE environment variable is not set.");

        string cookiesPath = Path.Combine(userProfile, "AppData", "Roaming", "Microsoft", "Windows", "Cookies");
        if (Directory.Exists(cookiesPath))
        {
            CleanDirectory(cookiesPath, "Cookies");
        }
    }

    private void CleanHistory()
    {
        string userProfile = Environment.GetEnvironmentVariable("USERPROFILE")
                             ?? throw new InvalidOperationException("USERPROFILE environment variable is not set.");

        string historyPath = Path.Combine(userProfile, "AppData", "Local", "Microsoft", "Windows", "History");
        if (Directory.Exists(historyPath))
        {
            CleanDirectory(historyPath, "History");
        }
    }

    private void CleanRemnantDriverFiles()
    {
        string userProfile = Environment.GetEnvironmentVariable("USERPROFILE")
                             ?? throw new InvalidOperationException("USERPROFILE environment variable is not set.");

        string[] driverPaths =
        {
            Path.Combine(userProfile, "AMD"),
            Path.Combine(userProfile, "NVIDIA"),
            Path.Combine(userProfile, "INTEL")
        };

        foreach (var driverPath in driverPaths)
        {
            if (Directory.Exists(driverPath))
            {
                CleanDirectory(driverPath, "Remnant driver files");
            }
        }
    }

    private void CleanTempFolder()
    {
        CleanDirectory(Path.GetTempPath(), "Temp folder");
    }

    private void CleanPrefetchFolder()
    {
        string windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string prefetchPath = Path.Combine(windowsPath, "Prefetch");
        CleanDirectory(prefetchPath, "Prefetch");
    }

    private void CleanWindowsTempFolder()
    {
        string windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string windowsTempPath = Path.Combine(windowsPath, "Temp");
        CleanDirectory(windowsTempPath, "Temp");
    }

    private void CleanLogFiles()
    {
        string windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        string[] logPaths =
        {
            Path.Combine(windowsPath, "Logs"),
            Path.Combine(windowsPath, "Panther"),
            Path.Combine(windowsPath, "SoftwareDistribution", "Download"),
            Path.Combine(programDataPath, "Microsoft", "Windows", "WER", "Temp")
        };

        foreach (var logPath in logPaths)
        {
            if (Directory.Exists(logPath))
            {
                CleanDirectory(logPath, "Log files");
            }
        }
    }

    private void CleanEventLogs()
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

    private void ResetDnsResolverCache()
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

    private void EraseInternetExplorerHistory()
    {
        try
        {
            _logger.LogInformation("Attempting to erase Internet Explorer temporary data...");

            Process process = new Process();
            process.StartInfo.FileName = "rundll32.exe";
            process.StartInfo.Arguments = "inetcpl.cpl,ClearMyTracksByProcess 4351";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();

            _logger.LogInformation("Internet Explorer temporary data erased successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to erase Internet Explorer temporary data: {ex.Message}");
        }
    }

    private void RunDISMOperations()
    {
        try
        {
            _logger.LogInformation("Running DISM to clean old service pack files...");

            Process process = new Process();
            process.StartInfo.FileName = "dism.exe";
            process.StartInfo.Arguments = "/online /cleanup-Image /spsuperseded";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("DISM operation completed successfully.");
            }
            else
            {
                _logger.LogInformation($"DISM operation failed. Exit code: {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to run DISM operation: {ex.Message}");
        }
    }

    private void RunSystemFileChecker()
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
                    TryDeleteFile(file);
                }

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    TryDeleteDirectory(dir);
                }

                _logger.LogInformation($"{folderName} cleaned successfully.");
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (IOException ex) when (IsFileLocked(ex))
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error cleaning {folderName}");
        }
    }
    
    private void TryDeleteFile(FileInfo file)
    {
        const int MaxRetries = 1;
        const int DelayBetweenRetries = 500;

        for (int i = 0; i < MaxRetries; i++)
        {
            try
            {
                file.Delete();
                _logger.LogInformation($"File {file.FullName} deleted successfully.");
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
            }
            catch (IOException ex) when (IsFileLocked(ex))
            {
                Thread.Sleep(DelayBetweenRetries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {file.FullName}");
                return;
            }
        }

        _logger.LogWarning(
            $"Could not delete file {file.FullName} after {MaxRetries} attempts due to it being in use by another process.");
    }

    private void TryDeleteDirectory(DirectoryInfo dir)
    {
        try
        {
            dir.Delete(true);
            _logger.LogInformation($"Directory {dir.FullName} deleted successfully.");
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (IOException ex) when (IsFileLocked(ex))
        {
        }
        catch (IOException ex)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting directory {dir.FullName}");
        }
    }

    private void DeleteFilesByPattern(string directory, string pattern)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                var enumerationOptions = new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                };

                var files = Directory.GetFiles(directory, pattern, enumerationOptions);
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                        _logger.LogInformation($"File {file} deleted successfully.");
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (IOException ex) when (IsFileLocked(ex))
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error deleting file '{file}'");
                    }
                }

                _logger.LogInformation($"Files with pattern {pattern} deleted successfully in {directory}.");
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting files with pattern {pattern} in {directory}");
        }
    }

    private bool IsFileLocked(IOException exception)
    {
        int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
        return errorCode == 32 || errorCode == 33;
    }
}