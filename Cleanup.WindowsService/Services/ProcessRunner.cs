using System.Diagnostics;
using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Services;

public class ProcessRunner : IProcessRunner
{
    private readonly ILogger<ProcessRunner> _logger;

    public ProcessRunner(ILogger<ProcessRunner> logger)
    {
        _logger = logger;
    }

    public bool RunProcess(string fileName, string arguments)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false
            });

            if (process == null)
            {
                _logger.LogError("Failed to start process {FileName}", fileName);
                return false;
            }

            process.WaitForExit();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running process {FileName} with arguments {Arguments}", fileName, arguments);
            return false;
        }
    }

    public int RunProcessWithExitCode(string fileName, string arguments)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false
            });

            if (process == null)
            {
                _logger.LogError("Failed to start process {FileName}", fileName);
                return -1;
            }

            process.WaitForExit();
            return process.ExitCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running process {FileName} with arguments {Arguments}", fileName, arguments);
            return -1;
        }
    }
}