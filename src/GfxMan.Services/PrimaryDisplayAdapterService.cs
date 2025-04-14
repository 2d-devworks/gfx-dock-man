using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;

namespace GfxMan.Services;

public class PrimaryDisplayAdapterService : HasOnStatusChangedEventServiceBase, IPrimaryDisplayAdapterService
{
    private readonly ISystemDeviceInformationService<DisplayAdapterInfo> _displayAdapterInfoService;

    public PrimaryDisplayAdapterService(ISystemDeviceInformationService<DisplayAdapterInfo> displayAdapterInfoService)
    {
        _displayAdapterInfoService = displayAdapterInfoService;
        _displayAdapterInfoService.OnStatusChanged += OnDisplayAdaptersChanged;
        GetCurrentState();
    }

    public DisplayAdapterInfo PrimaryDisplayAdapter { get; private set; }
    
    public void WatchForChanges(CancellationToken cancellationToken)
    {
        if (_displayAdapterInfoService is IHasWatchForChanges watcher)
        {
            watcher.WatchForChanges(cancellationToken);
        }
    }
    
    private void OnDisplayAdaptersChanged(object sender, EventArgs e)
    {
        GetCurrentState();
    }

    private void GetCurrentState()
    {
        PrimaryDisplayAdapter = _displayAdapterInfoService.Devices.FirstOrDefault(n => n.IsOk);
    }

    public void Dispose()
    {
        _displayAdapterInfoService.OnStatusChanged -= OnDisplayAdaptersChanged;
    }
}