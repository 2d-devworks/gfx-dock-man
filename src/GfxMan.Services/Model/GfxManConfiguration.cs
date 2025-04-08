namespace GfxMan.Services.Model;

public class GfxManConfiguration
{
    public ICollection<GfxConfigOption> ConfigurationOptions { get; set; } = new List<GfxConfigOption>()
    {
        new ()
    };

    public string ActiveConfiguration { get; set; } = GfxConfigOption.DefaultConfigName;
    
    public string InstalledGamesSourceConfig { get; set; } = string.Empty;
    public ICollection<GameInfo> ConfiguredGames { get; set; } = new List<GameInfo>();
}