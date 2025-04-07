using System.Text.Json;
using GfxMan.Services.Model;

namespace GfxMan.Services.Helpers;

public static class FileHelper
{
    private static string GetAppDataPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GfxMan");
    }
    
    private static string GetSettingsFilePath()
    {
        return Path.Combine(GetAppDataPath(), "settings.json");
    }
    
    public static void WriteSettingsFile(MyConfiguration configuration) 
    {
        var path = GetSettingsFilePath();
        var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public static MyConfiguration LoadSettings()
    {
        if (File.Exists(GetSettingsFilePath()))
        {
            var json = File.ReadAllText(GetSettingsFilePath());
            return JsonSerializer.Deserialize<MyConfiguration>(json) ?? new MyConfiguration();
        }
        else
        {
            return new MyConfiguration();
        }
    }
}