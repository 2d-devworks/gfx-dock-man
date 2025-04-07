namespace GfxMan.Services.Model;

public class MyConfiguration
{
    public bool IsConfiguredAsDocked { get; set; }
    public string InstalledGamesSourceConfig { get; set; } = string.Empty;
    public ICollection<GameInfo> ConfiguredGames { get; set; } = new List<GameInfo>();
}