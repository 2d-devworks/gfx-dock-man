namespace GfxMan.Services.Interfaces;

public interface IHasWatchForChanges
{
    void WatchForChanges(CancellationToken cancellationToken);
}