using Microsoft.Win32;

public static class AppSettings
{
    // Save location: HKCU\Software\Yuuya\YuuyaPad
    private const string RegistryPath = @"Software\Yuuya\YuuyaPad";

    public static string SearchEngine { get; set; } = "Google";
    public static string CustomSearchUrl { get; set; } = "";

    public static void Save()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                key.SetValue("SearchEngine", SearchEngine);
                key.SetValue("CustomSearchUrl", CustomSearchUrl);
            }
        }
        catch
        {
            // If saving fails, it can be ignored or logged.
        }
    }

    public static void Load()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                if (key != null)
                {
                    SearchEngine = key.GetValue("SearchEngine", "Google").ToString();
                    CustomSearchUrl = key.GetValue("CustomSearchUrl", "").ToString();
                }
            }
        }
        catch
        {
            // If loading fails, use default value
        }
    }
}
