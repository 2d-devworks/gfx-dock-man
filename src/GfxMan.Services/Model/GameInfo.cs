namespace GfxMan.Services.Model;

public class GameInfo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<string> SettingsFiles { get; set; } = [];
}