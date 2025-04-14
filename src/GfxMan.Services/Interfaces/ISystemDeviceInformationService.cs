namespace GfxMan.Services.Interfaces;

public interface ISystemDeviceInformationService<out TSystemDevice>: IHasOnStatusChangedEvent where TSystemDevice : class
{
    IEnumerable<TSystemDevice> Devices { get; }
    
    void EnumerateDevices();
}