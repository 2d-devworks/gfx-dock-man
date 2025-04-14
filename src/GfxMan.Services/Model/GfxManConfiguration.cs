namespace GfxMan.Services.Model;

public class GfxManConfiguration
{
    public bool ShouldWatchForChanges { get; set; } = true;
    
    public ICollection<GfxConfigOption> ConfigurationOptions { get; set; } = new List<GfxConfigOption>()
    {
        new ()
    };

    public string ActiveConfiguration { get; set; } = GfxConfigOption.DefaultConfigName;
    public ICollection<GameInfo> ConfiguredGames { get; set; } = new List<GameInfo>();
}