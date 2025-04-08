using System.Collections.Concurrent;

namespace GfxMan.Services.Interfaces;

public interface IDisplayAdapterService
{
    ConcurrentDictionary<string, bool> DisplayAdapters { get; }
    int PrimaryDisplayWidth { get; }
    int PrimaryDisplayHeight { get; }
    
    event EventHandler OnStatusChanged;
    
    void WatchForChanges(CancellationToken cancellationToken);
}