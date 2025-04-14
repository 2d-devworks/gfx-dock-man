using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.Logging;

namespace GfxMan.Services;

public class PrimaryMonitorService: HasOnStatusChangedEventServiceBase, IPrimaryMonitorService
{
    private readonly ILogger<PrimaryMonitorService> _logger;
    private readonly ISystemDeviceInformationService<MonitorInfo> _monitorInfoService;

    public PrimaryMonitorService(ILogger<PrimaryMonitorService> logger, ISystemDeviceInformationService<MonitorInfo> monitorInfoService)
    {
        _logger = logger;
        _monitorInfoService = monitorInfoService;
        _monitorInfoService.OnStatusChanged += OnDisplaySettingsChanged;
        GetCurrentState();
    }
    
    public MonitorInfo PrimaryMonitor { get; private set; } = new ();

    private void OnDisplaySettingsChanged(object sender, EventArgs e)
    {
        GetCurrentState();
    }
    
    private void GetCurrentState()
    {
        var monitor = _monitorInfoService.Devices.FirstOrDefault(n => n.IsPrimaryDisplay);
        if (monitor == null)
        {
            _logger.LogError("Primary monitor not found.");
            return;
        }
        
        if (PrimaryMonitor.MaximumPixelWidth == monitor.MaximumPixelWidth && PrimaryMonitor.MaximumPixelHeight == monitor.MaximumPixelHeight)
        {
            PrimaryMonitor.Name = monitor.Name;
            return;
        }
        
        PrimaryMonitor = monitor;
        
        _logger.LogInformation($"Primary monitor changed: {PrimaryMonitor.MaximumPixelWidth}x{PrimaryMonitor.MaximumPixelHeight}");
        InvokeStatusChangedEvent(EventArgs.Empty);
    }
    
    public void Dispose()
    {
        _monitorInfoService.OnStatusChanged -= OnDisplaySettingsChanged;
    }
}