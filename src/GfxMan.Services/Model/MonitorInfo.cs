namespace GfxMan.Services.Model;

public class MonitorInfo
{
    public string? Name { get; set; }

    public uint MaximumPixelWidth { get; set; } = 1920;
    
    public uint MaximumPixelHeight { get; set; } = 1080;
    
    public bool IsPrimaryDisplay { get; set; }
}