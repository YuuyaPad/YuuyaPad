using Microsoft.Win32;
using System;
using System.Drawing;

public static class AppSettings
{
    // Saved in HKCU\Software\Yuuya\YuuyaPad
    private const string RegistryPath = @"Software\Yuuya\YuuyaPad";

    public static string SearchEngine { get; set; } = "Google";
    public static string CustomSearchUrl { get; set; } = "";
    public static string FontName { get; set; } = SystemFonts.DefaultFont.Name;
    public static float FontSize { get; set; } = SystemFonts.DefaultFont.Size;
    public static FontStyle FontStyle { get; set; } = SystemFonts.DefaultFont.Style;
    public static bool ShowStatusBar { get; set; } = true;

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
                key.SetValue("ShowStatusBar", ShowStatusBar ? 1 : 0, RegistryValueKind.DWord);
            }
        }
        catch
        {
            // Ignore or log
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
                    FontSize = Convert.ToSingle(key.GetValue("FontSize", SystemFonts.DefaultFont.Size));
                    FontStyle = (FontStyle)Convert.ToInt32(key.GetValue("FontStyle", (int)SystemFonts.DefaultFont.Style));
                    ShowStatusBar = Convert.ToInt32(key.GetValue("ShowStatusBar", 1)) != 0;
                }
            }
        }
        catch
        {
            // Use defaults if loading fails
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

    public static void SetFont(Font font)
    {
        FontName = font.Name;
        FontSize = font.Size;
        FontStyle = font.Style;
        Save();
    }
}