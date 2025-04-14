using System.IO.Abstractions;
using System.Text.Json;
using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.Logging;

namespace GfxMan.Services;

public class SettingsFileService(IFileSystem fileSystem, ILogger<SettingsFileService> logger) : ISettingsFileService
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

    public void SwitchGameConfiguration(GameInfo game, string fromConfig, string toConfig)
    {
        var configFiles = game.SettingsFiles;
        foreach (var configFile in configFiles)
        {
            if (string.IsNullOrWhiteSpace(configFile))
            {
                continue;
            }
            
            if (fileSystem.File.Exists(configFile))
            {
                fileSystem.File.Copy(configFile, $"{configFile}.{fromConfig}", true);
            }
        
            if (fileSystem.File.Exists($"{configFile}.{toConfig}"))
            {
                fileSystem.File.Copy($"{configFile}.{toConfig}", configFile, true);
            }
        }
    }

    public void RenameAllGameConfigurationOptionFiles(GfxManConfiguration configuration, string fromConfig, string toConfig)
    {
        foreach (var game in configuration.ConfiguredGames)
        {
            RenameGameSettingsFilesForConfig(game, fromConfig, toConfig);
        }
    }
    
    private void RenameGameSettingsFilesForConfig(GameInfo gameInfo, string fromConfig, string toConfig)
    {
        var configFiles = gameInfo.SettingsFiles;
        foreach (var configFile in configFiles)
        {
            if (string.IsNullOrWhiteSpace(configFile))
            {
                continue;
            }

            try
            {
                var oldName = $"{configFile}.{fromConfig}";
                var newName = $"{configFile}.{toConfig}";
                if (!fileSystem.File.Exists(oldName))
                {
                    continue;
                }

                fileSystem.File.Move(oldName, newName);
                logger.LogInformation($"{gameInfo.Name}:  renamed settings file {fromConfig} to {toConfig}.");
                
            }
            catch
            {
                logger.LogError($"An error occured renaming settings for game {gameInfo.Name} from {fromConfig} to {toConfig}");
            }
        }
    }
}