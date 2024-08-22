namespace GfxMan.Model;

public class MyConfiguration
{
    public bool IsConfiguredAsDocked { get; set; }
    public string InstalledGamesSourceConfig { get; set; } = string.Empty;
    public IEnumerable<GameInfo> ConfiguredGames { get; set; } = new List<GameInfo>();
}