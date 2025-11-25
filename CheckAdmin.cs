using System.Security.Principal;

public static class CheckAdmin
{
    /// <summary>
    /// Check if the current process is running with administrator privileges
    /// </summary>
    public static bool IsRunAsAdmin()
    {
        try
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}
