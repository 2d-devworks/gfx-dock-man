using System.IO.Abstractions;
using System.Text.Json;
using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;

namespace GfxMan.Services;

public class SettingsFileService(IFileSystem fileSystem) : ISettingsFileService
{
    private const string AppDataFolder = "2d-devworks\\GfxMan";
    private const string SettingsFileName = "settings.json";
    
    private string GetAppDataPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return fileSystem.Path.Combine(appDataPath, AppDataFolder);
    }
    
    private string GetSettingsFilePath()
    {
        return fileSystem.Path.Combine(GetAppDataPath(), SettingsFileName);
    }

    public void SaveConfiguration(GfxManConfiguration configuration)
    {
        var directory = GetAppDataPath();
        if (!fileSystem.Directory.Exists(directory))
        {
            fileSystem.Directory.CreateDirectory(directory!);
        }
        
        var path = GetSettingsFilePath();
        var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true });
        fileSystem.File.WriteAllText(path, json);
    }

    public GfxManConfiguration LoadConfiguration()
    {
        var settingsFilePath = GetSettingsFilePath();
        if (fileSystem.File.Exists(settingsFilePath))
        {
            var json = fileSystem.File.ReadAllText(settingsFilePath);
            return JsonSerializer.Deserialize<GfxManConfiguration>(json) ?? new GfxManConfiguration();
        }
        else
        {
            return new GfxManConfiguration();
        }
    }
}