namespace Cleanup.WindowsService.Interfaces;

public interface IProcessRunner
{
    bool RunProcess(string fileName, string arguments);
    int RunProcessWithExitCode(string fileName, string arguments);
}
