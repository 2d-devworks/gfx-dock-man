using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.Logging;

namespace GfxMan.Services;

public class PrimaryDisplayAdapterService : HasOnStatusChangedEventServiceBase, IPrimaryDisplayAdapterService
{
    private readonly ILogger<PrimaryDisplayAdapterService> _logger;
    private readonly ISystemDeviceInformationService<DisplayAdapterInfo> _displayAdapterInfoService;

    public PrimaryDisplayAdapterService(ILogger<PrimaryDisplayAdapterService> logger, ISystemDeviceInformationService<DisplayAdapterInfo> displayAdapterInfoService)
    {
        _logger = logger;
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
        var lastPrimaryAdapter = PrimaryDisplayAdapter;
        var newPrimaryAdapter = _displayAdapterInfoService.Devices.FirstOrDefault(n => n.IsOk);
        if (newPrimaryAdapter == null || newPrimaryAdapter == lastPrimaryAdapter)
        {
            return;
        }
        PrimaryDisplayAdapter = newPrimaryAdapter;
        _logger.LogInformation($"Primary display adapter changed: {PrimaryDisplayAdapter?.Name}");
        InvokeStatusChangedEvent(EventArgs.Empty);
    }

    public void Dispose()
    {
        _displayAdapterInfoService.OnStatusChanged -= OnDisplayAdaptersChanged;
    }
}