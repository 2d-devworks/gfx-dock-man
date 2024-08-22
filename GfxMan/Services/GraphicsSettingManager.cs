using System.Management;
using System.Text.Json;
using GfxMan.Model;
using Microsoft.Extensions.Configuration;

namespace GfxMan.Services;

#pragma warning disable CA1416
public class GraphicsSettingManager
{
    private bool _isDocked;
    private readonly IConfiguration _configuration;

    private MyConfiguration MyConfiguration =>
        _configuration.Get<MyConfiguration>() ?? new MyConfiguration();

    public GraphicsSettingManager(IConfiguration configuration, CancellationToken cancellationToken)
    {
        _isDocked = GetGfxCount() > 1;
        _configuration = configuration;
        
    }
    
    public async Task Scan(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var gfxCount = GetGfxCount();
            var isDocked = gfxCount > 1;
            
            if (_isDocked != isDocked)
            {
                if (isDocked)
                {
                    SetToDocked();
                }
                else
                {
                    SetToUndocked();
                }
                MyConfiguration.IsConfiguredAsDocked = isDocked;
                SaveConfig();
            }
            
            _isDocked = isDocked;
            
            Console.WriteLine($"Found {gfxCount} graphics devices");
            
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
    }
    
    public void SetToDocked()
    {
        _isDocked = true;
        foreach(var game in MyConfiguration.ConfiguredGames)
        {
            SwitchGameToDocked(game);
        }
    }
    
    public void SetToUndocked()
    {
        _isDocked = false;
        foreach(var game in MyConfiguration.ConfiguredGames)
        {
            SwitchGameToUndocked(game);
        }
    }
    
    private static int GetGfxCount()
    {
        return new ManagementObjectSearcher("select * from Win32_VideoController").Get().Count;
    }

    private static void SwitchGameToDocked(GameInfo gameInfo)
    {
        SwitchGame(gameInfo, isToDocked: true);
    }
    
    private static void SwitchGameToUndocked(GameInfo gameInfo)
    {
        SwitchGame(gameInfo, isToDocked: false);
    }

    private static void SwitchGame(GameInfo game, bool isToDocked)
    {
        var fromPrefix = isToDocked ? "un" : "";
        var toPrefix = isToDocked ? "" : "un";

        var configFile = game.SettingsFile;
        if (File.Exists(configFile))
        {
            File.Copy(configFile, $"{configFile}.{fromPrefix}docked", true);
        }

        if (File.Exists($"{configFile}.{toPrefix}docked"))
        {
            File.Copy($"{configFile}.{toPrefix}docked", configFile, true);
        }

        Console.WriteLine($"Switched {game.Name} to {toPrefix}docked mode.");
    }

    private void SaveConfig()
    {
        var config = new MyConfiguration
        {
            IsConfiguredAsDocked = _isDocked,
            ConfiguredGames = MyConfiguration.ConfiguredGames,
            InstalledGamesSourceConfig = MyConfiguration.InstalledGamesSourceConfig
        };

        var json = JsonSerializer.Serialize(config);
        File.WriteAllText("appsettings.json", json);
    }
}