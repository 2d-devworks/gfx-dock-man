using GfxMan.Services.Model;

namespace GfxMan.Services.Interfaces;

public interface IBackupFilesService
{
    void BackupGameSettingsFiles(GfxManConfiguration configuration);
    
    void RestoreGameSettingsFiles(GfxManConfiguration configuration, string activeConfig, Guid? backupId = null);
}