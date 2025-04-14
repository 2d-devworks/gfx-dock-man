
using GfxMan.Services.Model;

namespace GfxMan.Services.Interfaces;

public interface IPrimaryMonitorService: IDisposable, IHasOnStatusChangedEvent
{
    MonitorInfo PrimaryMonitor { get; }
}