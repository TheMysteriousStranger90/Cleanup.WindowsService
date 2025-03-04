using System.Runtime.InteropServices;
using Cleanup.WindowsService.Interfaces;

namespace Cleanup.WindowsService.Tasks;

public class RecycleBinCleanupTask : BaseCleanupTask
{
    public override string Name => "Recycle Bin Cleanup";

    [DllImport("Shell32.dll")]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);

    private const uint SHERB_NOCONFIRMATION = 0x00000001;
    private const uint SHERB_NOPROGRESSUI = 0x00000002;
    private const uint SHERB_NOSOUND = 0x00000004;

    public RecycleBinCleanupTask(ILogger<RecycleBinCleanupTask> logger, IFileOperations fileOps)
        : base(logger, fileOps)
    {
    }

    protected override bool ExecuteCore()
    {
        try
        {
            uint flags = SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND;
            SHEmptyRecycleBin(IntPtr.Zero, null, flags);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error emptying recycle bin");
            return false;
        }
    }
}