using GfxMan.Services.Model;

namespace GfxMan.Services.Interfaces;

public interface IPrimaryDisplayAdapterService: IHasOnStatusChangedEvent, IHasWatchForChanges, IDisposable
{
    DisplayAdapterInfo PrimaryDisplayAdapter { get; }
}