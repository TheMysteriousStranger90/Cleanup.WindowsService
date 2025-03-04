namespace Cleanup.WindowsService.Interfaces;

public interface ICleanupTask
{
    string Name { get; }
    bool Execute();
}