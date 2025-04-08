namespace GfxMan.Services.Model;

public class GfxManConfiguration
{
    public ICollection<GfxConfigOption> ConfigurationOptions { get; set; } = new List<GfxConfigOption>();
    
    public string ActiveConfiguration { get; set; }
    
    public string InstalledGamesSourceConfig { get; set; } = string.Empty;
    public ICollection<GameInfo> ConfiguredGames { get; set; } = new List<GameInfo>();
}