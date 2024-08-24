using System.Security.Principal;

namespace Cleanup.WindowsService;

public static class PrivilegeManager
{
    public static bool IsAdministrator(string programName)
    {
        try
        {
            var identity = WindowsIdentity.GetCurrent();
            if (identity == null)
            {
                return false;
            }

            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static bool EnsureAdminPrivileges(bool isService, string programName)
    {
        if (IsAdministrator(programName))
        {
            return true;
        }

        return false;
    }
}