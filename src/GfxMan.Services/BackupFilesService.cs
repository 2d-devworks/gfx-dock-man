using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.Logging;

namespace GfxMan.Services;

public class BackupFilesService(ILogger<BackupFilesService> logger, IAppFileSystem fileSystem): IBackupFilesService
{
    private string GetBackupFolderPath(Guid? backupId = null)
    {
        var path = fileSystem.Path.Combine(fileSystem.GetAppDataPath(), "backup");
        return backupId.HasValue ?  fileSystem.Path.Combine(path, $"{backupId}") : path;
    }
    
    public void BackupGameSettingsFiles(GfxManConfiguration configuration)
    {
        var backupId = Guid.NewGuid();
        var folder = GetBackupFolderPath(backupId);
        
        if (string.IsNullOrWhiteSpace(folder))
        {
            return;
        }
        
        if (!fileSystem.Directory.Exists(folder))
        {
            fileSystem.Directory.CreateDirectory(folder);
        }

        var configOptions = configuration.ConfigurationOptions.Select(n => n.Name).ToList();
        foreach (var gameInfo in configuration.ConfiguredGames)
        {
            BackupGameFiles(folder, gameInfo, configOptions);
        }
    }

    private void BackupGameFiles(string backupFolder, GameInfo gameInfo, ICollection<string> configNames)
    {
        var gameFolder = fileSystem.Path.Combine(backupFolder, gameInfo.Id.ToString());
        if (!fileSystem.Directory.Exists(gameFolder))
        {
            fileSystem.Directory.CreateDirectory(gameFolder);
        }

        var filesToBackup = from settingsFile in gameInfo.SettingsFiles
            from config in configNames
            select new { settingsFile, config };

        foreach (var file in filesToBackup)
        {
            var pathToBackup = $"{file.settingsFile}.{file.config}";
            var folder = fileSystem.Path.GetDirectoryName(pathToBackup);
            if (!fileSystem.Directory.Exists(folder) && !fileSystem.File.Exists(pathToBackup))
            {
                continue;
            }
            var fileName = fileSystem.Path.GetFileName(pathToBackup);
            fileSystem.File.Copy(pathToBackup, fileSystem.Path.Combine(gameFolder,fileName), true);
        }
    }

    public void RestoreGameSettingsFiles(GfxManConfiguration configuration, string activeConfig, Guid? backupId = null)
    {
        string? backupFolder;
        if (!backupId.HasValue)
        {
            backupFolder = new DirectoryInfo(GetBackupFolderPath())
                .GetDirectories()
                .OrderByDescending(d => d.CreationTime)
                .FirstOrDefault()?.FullName; 
        }
        else
        {
            backupFolder = GetBackupFolderPath(backupId.Value);
        }

        if (string.IsNullOrWhiteSpace(backupFolder))
        {
            return;
        }
        
        var configOptions = configuration.ConfigurationOptions.Select(n => n.Name).ToList();
        foreach (var gameInfo in configuration.ConfiguredGames)
        {
            RestoreGameFiles(backupFolder!, gameInfo, configOptions, activeConfig);
        }
    }

    private void RestoreGameFiles(string backupFolder, GameInfo gameInfo, ICollection<string> configNames, string activeConfig)
    {
        var gameFolder = fileSystem.Path.Combine(backupFolder, gameInfo.Id.ToString());
        if (!fileSystem.Directory.Exists(gameFolder))
        {
            return;
        }

        var filesToRestore = from settingsFile in gameInfo.SettingsFiles
            from config in configNames
            select new { settingsFile, config };

        foreach (var file in filesToRestore)
        {
            var pathToRestoreTo = $"{file.settingsFile}.{file.config}";
            var fileName = fileSystem.Path.GetFileName(pathToRestoreTo);
            var sourceFile = fileSystem.Path.Combine(gameFolder, fileName);
            if (!fileSystem.File.Exists(sourceFile))
            {
                continue;
            }
            fileSystem.File.Copy(sourceFile, pathToRestoreTo, true);
            if (file.config == activeConfig)
            {
                fileSystem.File.Copy(sourceFile, file.settingsFile, true);
            }
        }
    }
}