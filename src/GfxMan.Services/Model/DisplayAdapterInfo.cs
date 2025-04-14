namespace GfxMan.Services.Model;

public class DisplayAdapterInfo
{
    public string Name { get; set; } = GfxConfigOption.DefaultDisplayDriverName;
    public bool IsOk { get; set; } = true;
}