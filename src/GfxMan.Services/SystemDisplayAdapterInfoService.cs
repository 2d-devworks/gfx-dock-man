using System.Management;
using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.Logging;

namespace GfxMan.Services;

public class SystemDisplayAdapterInfoService : HasOnStatusChangedEventServiceBase, ISystemDeviceInformationService<DisplayAdapterInfo>, IHasWatchForChanges
{
    private readonly ILogger<SystemDisplayAdapterInfoService> _logger;
    public IEnumerable<DisplayAdapterInfo> Devices { get; private set; } = new List<DisplayAdapterInfo>();

    public SystemDisplayAdapterInfoService(ILogger<SystemDisplayAdapterInfoService> logger)
    {
        _logger = logger;
        EnumerateDevices();
    }
    
    public void EnumerateDevices()
    {
        var devices = new List<DisplayAdapterInfo>();
        var adapters = new ManagementObjectSearcher("select * from Win32_VideoController").Get();
        foreach (var adapter in adapters)
        {
            var device = new DisplayAdapterInfo()
            {
                Name = adapter["Name"].ToString(),
                IsOk = AdapterIsOk(adapter)
            };
            devices.Add(device);
        }
        Devices = devices;
    }
    
    private static bool AdapterIsOk(ManagementBaseObject adapter)
    {
        return adapter["Status"].ToString() == "OK";
    }

    public void WatchForChanges(CancellationToken cancellationToken)
    {
        const string query = "SELECT * FROM __InstanceModificationEvent " +
                             "WITHIN 5 " +
                             "WHERE TargetInstance ISA 'Win32_VideoController'";
        
        var watcher = new ManagementEventWatcher(query);
        watcher.Options.Timeout = new TimeSpan(0, 0, 5);

        _logger.LogInformation($"Display adapter watcher starting...");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var evt = watcher.WaitForNextEvent();
                if (evt == null)
                {
                    continue;
                }

                EnumerateDevices();
                InvokeStatusChangedEvent(EventArgs.Empty);
            }
            catch (ManagementException mex) when (mex.ErrorCode is ManagementStatus.OperationCanceled or ManagementStatus.Timedout)
            {
                // Ignore; likely due to disposal, cancellation, or programmed timeout
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while watching for changes: {ex.Message}");
            }
        }
        
        _logger.LogInformation($"Display adapter watcher stopped...");
    }
}