using GfxMan.Services.Interfaces;

namespace GfxMan.Services;

public abstract class HasOnStatusChangedEventServiceBase: IHasOnStatusChangedEvent
{
    private event EventHandler StatusChangedEvent;
    private readonly object _objectLock = new ();
    
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

    protected void InvokeStatusChangedEvent(EventArgs args)
    {
        StatusChangedEvent?.Invoke(this, args);
    }
}