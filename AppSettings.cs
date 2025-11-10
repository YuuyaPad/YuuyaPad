using Microsoft.Win32;
using System.Drawing;

public static class AppSettings
{
    private const string RegistryPath = @"Software\Yuuya\YuuyaPad";

    public static string SearchEngine { get; set; } = "Google";
    public static string CustomSearchUrl { get; set; } = "";

    public static string FontName { get; set; } = SystemFonts.DefaultFont.Name;
    public static float FontSize { get; set; } = SystemFonts.DefaultFont.SizeInPoints;
    public static FontStyle FontStyle { get; set; } = SystemFonts.DefaultFont.Style;

    public static void Save()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                key.SetValue("SearchEngine", SearchEngine);
                key.SetValue("CustomSearchUrl", CustomSearchUrl);
                key.SetValue("FontName", FontName);
                key.SetValue("FontSize", FontSize);
                key.SetValue("FontStyle", (int)FontStyle);
            }
        }
        catch
        {
            // Ignore if save fails
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
                    FontName = key.GetValue("FontName", SystemFonts.DefaultFont.Name).ToString();
                    FontSize = float.Parse(key.GetValue("FontSize", SystemFonts.DefaultFont.SizeInPoints).ToString());
                    FontStyle = (FontStyle)(int)key.GetValue("FontStyle", (int)SystemFonts.DefaultFont.Style);
                }
            }
        }
        catch
        {
            // If loading fails, use default value
        }
    }

    public static Font GetFont()
    {
        try
        {
            return new Font(FontName, FontSize, FontStyle);
        }
        catch
        {
            return SystemFonts.DefaultFont;
        }
    }
}
