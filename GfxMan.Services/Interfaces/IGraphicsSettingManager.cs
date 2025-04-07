using GfxMan.Services.Model;

namespace GfxMan.Services.Interfaces;

public interface IGraphicsSettingManager
{
    bool IsDocked { get; }
    
    string StatusText { get; }
    
    MyConfiguration Configuration { get; set; }

    event EventHandler OnStatusChanged;

    Task Scan(CancellationToken cancellationToken);
    void SetToDocked();
    void SetToUndocked();
    void SaveConfig();
}