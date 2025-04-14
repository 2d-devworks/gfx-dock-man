using GfxMan.Services.Model;

namespace GfxMan.Services.Interfaces;

public interface ISettingsFileService
{
    void SaveConfiguration(GfxManConfiguration configuration);
    
    GfxManConfiguration LoadConfiguration();
    
    void SwitchGameConfiguration(GameInfo game, string fromConfig, string toConfig);
    
    void RenameAllGameConfigurationOptionFiles(GfxManConfiguration configuration, string fromConfig, string toConfig);
}