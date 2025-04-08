using GfxMan.Services.Model;

namespace GfxMan.Services.Interfaces;

public interface IGraphicsSettingManager
{
    string ActiveConfig { get; }
    
    GfxManConfiguration Configuration { get; set; }

    event EventHandler OnStatusChanged;

    Task Scan(CancellationToken cancellationToken);
    
    void ChangeActiveConfiguration(string newActiveConfiguration);
    
    void SaveConfig();
}