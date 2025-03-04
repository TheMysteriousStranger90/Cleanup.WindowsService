using System.Management;
using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class SystemRestorePointsTask : BaseCleanupTask
{
    public override string Name => "System Restore Points Management";

    public SystemRestorePointsTask(
        ILogger<SystemRestorePointsTask> logger,
        IFileOperations fileOps) : base(logger, fileOps)
    {
    }

    protected override bool ExecuteCore()
    {
        try
        {
            _logger.LogInformation("Managing system restore points...");

            const int pointsToKeep = 1;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM SystemRestore");
            ManagementObjectCollection restorePoints = searcher.Get();

            if (restorePoints.Count <= pointsToKeep)
            {
                _logger.LogInformation("No excess restore points to clean (total: {Count})", restorePoints.Count);
                return true;
            }

            var sortedPoints = new List<(uint SequenceNumber, DateTime CreationTime)>();

            foreach (ManagementObject restorePoint in restorePoints)
            {
                string creationTimeStr = restorePoint["CreationTime"].ToString();
                if (DateTime.TryParse(creationTimeStr, out DateTime creationTime))
                {
                    sortedPoints.Add((Convert.ToUInt32(restorePoint["SequenceNumber"]), creationTime));
                }
            }

            sortedPoints.Sort((a, b) => b.CreationTime.CompareTo(a.CreationTime));

            for (int i = pointsToKeep; i < sortedPoints.Count; i++)
            {
                ManagementClass systemRestore = new ManagementClass("SystemRestore");
                systemRestore.InvokeMethod("Remove", new object[] { sortedPoints[i].SequenceNumber });
                _logger.LogInformation(
                    "Removed restore point with sequence number {SequenceNumber} from {CreationTime}",
                    sortedPoints[i].SequenceNumber,
                    sortedPoints[i].CreationTime);
            }

            _logger.LogInformation("System restore points managed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to manage system restore points");
            return false;
        }
    }
}