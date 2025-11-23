using Microsoft.Win32;
using System;
using System.Drawing;

public static class AppSettings
{
    // Saved in HKCU\Software\Yuuya\YuuyaPad
    private const string MainKey = @"Software\Yuuya\YuuyaPad";
    private const string WindowStateKey = @"Software\Yuuya\YuuyaPad\WindowState";

    public static string SearchEngine { get; set; } = "Google";
    public static string CustomSearchUrl { get; set; } = "";
    public static string FontName { get; set; } = SystemFonts.DefaultFont.Name;
    public static float FontSize { get; set; } = SystemFonts.DefaultFont.Size;
    public static FontStyle FontStyle { get; set; } = SystemFonts.DefaultFont.Style;
    public static bool ShowStatusBar { get; set; } = true;

    public static int KeepWindowSize { get; set; } = 0;

    public static int WindowX { get; set; } = 100;
    public static int WindowY { get; set; } = 100;
    public static int WindowWidth { get; set; } = 800;
    public static int WindowHeight { get; set; } = 600;
    public static bool WindowMaximized { get; set; } = false;

    public static void Save()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(MainKey))
            {
                key.SetValue("SearchEngine", SearchEngine);
                key.SetValue("CustomSearchUrl", CustomSearchUrl);
                key.SetValue("FontName", FontName);
                key.SetValue("FontSize", FontSize);
                key.SetValue("FontStyle", (int)FontStyle);
                key.SetValue("ShowStatusBar", ShowStatusBar ? 1 : 0, RegistryValueKind.DWord);

                key.SetValue("KeepWindowSize", KeepWindowSize, RegistryValueKind.DWord);
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
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(MainKey))
            {
                if (key != null)
                {
                    SearchEngine = key.GetValue("SearchEngine", "Google").ToString();
                    CustomSearchUrl = key.GetValue("CustomSearchUrl", "").ToString();
                    FontName = key.GetValue("FontName", SystemFonts.DefaultFont.Name).ToString();
                    FontSize = Convert.ToSingle(key.GetValue("FontSize", SystemFonts.DefaultFont.Size));
                    FontStyle = (FontStyle)Convert.ToInt32(key.GetValue("FontStyle", (int)SystemFonts.DefaultFont.Style));
                    ShowStatusBar = Convert.ToInt32(key.GetValue("ShowStatusBar", 1)) != 0;

                    KeepWindowSize = Convert.ToInt32(key.GetValue("KeepWindowSize", 0));
                }
            }
        }
        catch
        {
            // Use defaults
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

    public static void SaveWindowSize()
    {
        if (KeepWindowSize != 1) return;

        try
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(WindowStateKey))
            {
                key.SetValue("WindowX", WindowX, RegistryValueKind.DWord);
                key.SetValue("WindowY", WindowY, RegistryValueKind.DWord);
                key.SetValue("WindowWidth", WindowWidth, RegistryValueKind.DWord);
                key.SetValue("WindowHeight", WindowHeight, RegistryValueKind.DWord);
                key.SetValue("WindowMaximized", WindowMaximized ? 1 : 0, RegistryValueKind.DWord);
            }
        }
        catch { }
    }

    public static void LoadWindowSize()
    {
        if (KeepWindowSize != 1) return;

        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(WindowStateKey))
            {
                if (key == null) return;

                WindowX = Convert.ToInt32(key.GetValue("WindowX", WindowX));
                WindowY = Convert.ToInt32(key.GetValue("WindowY", WindowY));
                WindowWidth = Convert.ToInt32(key.GetValue("WindowWidth", WindowWidth));
                WindowHeight = Convert.ToInt32(key.GetValue("WindowHeight", WindowHeight));
                WindowMaximized = Convert.ToInt32(key.GetValue("WindowMaximized", 0)) == 1;
            }
        }
        catch
        {

        }
    }
}