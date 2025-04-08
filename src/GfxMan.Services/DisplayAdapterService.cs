using System.Collections.Concurrent;
using System.Management;
using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.Logging;

namespace GfxMan.Services;

public class DisplayAdapterService : IDisplayAdapterService
{
    private event EventHandler StatusChangedEvent;
    private readonly object _objectLock = new ();
    private readonly ILogger<DisplayAdapterService> _logger;
    
    

    public DisplayAdapterService(ILogger<DisplayAdapterService> logger)
    {
        _logger = logger;
        
        var gfxConfigOption = new GfxConfigOption();
        PrimaryDisplayWidth = gfxConfigOption.PrimaryDisplayWidth;
        PrimaryDisplayHeight = gfxConfigOption.PrimaryDisplayHeight;
        
        var adapters = new ManagementObjectSearcher("select * from Win32_VideoController").Get();
        DisplayAdapters = new ConcurrentDictionary<string, bool>();
        
        foreach (var adapter in adapters)
        {
            DisplayAdapters.AddOrUpdate(adapter["Name"].ToString(), adapter["Status"].ToString() == "OK", (_, oldValue) => oldValue);
        }
    }

    public ConcurrentDictionary<string, bool> DisplayAdapters { get; }
    public int PrimaryDisplayWidth { get; }
    public int PrimaryDisplayHeight { get; }
    
    public event EventHandler OnStatusChanged
    {
        add
        {
            lock (_objectLock)
            {
                StatusChangedEvent += value;
            }
        }
        remove
        {
            lock (_objectLock)
            {
                StatusChangedEvent -= value;
            }
        }
    }

    public void WatchForChanges(CancellationToken cancellationToken)
    {
        const string query = "SELECT * FROM __InstanceModificationEvent " +
                             "WITHIN 5 " +
                             "WHERE TargetInstance ISA 'Win32_VideoController'";
        
        var watcher = new ManagementEventWatcher(query);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var evt = watcher.WaitForNextEvent();
                if (evt == null)
                {
                    continue;
                }
                var instance = (ManagementBaseObject)evt["TargetInstance"];
                var name = instance["Name"]?.ToString();
                var status = instance["Status"]?.ToString();

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                
                DisplayAdapters.AddOrUpdate(name, status == "OK", (_, _) => status == "OK");
                StatusChangedEvent?.Invoke(this, EventArgs.Empty);

                _logger.LogInformation($"Display adapter changed: {name}, Status: {status}");
            }
            catch (ManagementException mex) when (mex.ErrorCode == ManagementStatus.OperationCanceled)
            {
                // Ignore; likely due to disposal
            }
        }
    }
}