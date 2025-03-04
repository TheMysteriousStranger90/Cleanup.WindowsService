namespace Cleanup.WindowsService.Interfaces;

public interface ICleanupTaskFactory
{
    IEnumerable<ICleanupTask> CreateTasks();
}