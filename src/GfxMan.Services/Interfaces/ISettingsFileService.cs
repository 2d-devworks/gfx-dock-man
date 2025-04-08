using GfxMan.Services.Model;

namespace GfxMan.Services.Interfaces;

public interface ISettingsFileService
{
    void SaveConfiguration(GfxManConfiguration configuration);
    
    GfxManConfiguration LoadConfiguration();
}