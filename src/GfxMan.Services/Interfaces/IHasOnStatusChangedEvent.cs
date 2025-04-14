namespace GfxMan.Services.Interfaces;

public interface IHasOnStatusChangedEvent
{
    event EventHandler OnStatusChanged;
}