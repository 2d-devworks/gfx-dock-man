using GfxMan.Services.Model;

namespace GfxMan.Services.Interfaces;

public interface IGraphicsSettingManager: IDisposable, IHasOnStatusChangedEvent
{
    string ActiveConfig { get; }
    
    string CurrentDisplayAdapter { get; }
    uint CurrentDisplayMaxWidth { get; }
    uint CurrentDisplayMaxHeight { get; }
    
    GfxManConfiguration Configuration { get; set; }

    Task WatchForChanges(CancellationToken cancellationToken);
    
    void ChangeActiveConfiguration(string newActiveConfiguration);
    
    void RenameConfigurationOption(string fromConfig, string toConfig);
    
    void SaveConfig();
}