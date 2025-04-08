namespace GfxMan.Services.Model;

public class GfxConfigOption
{
    public const string DefaultConfigName = "Default";
    public const string DefaultDisplayDriverName = "AMD Radeon (TM) 780M Graphics";
    
    public string Name { get; set; } = DefaultConfigName;
    public string PrimaryDisplayDriverName { get; set; } = DefaultDisplayDriverName;
    public int PrimaryDisplayWidth { get; set; } = 1920;
    public int PrimaryDisplayHeight { get; set; } = 1080;
}