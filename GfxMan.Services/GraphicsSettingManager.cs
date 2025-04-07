using System.Management;
using GfxMan.Services.Helpers;
using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.Logging;

namespace GfxMan.Services;

public class GraphicsSettingManager : IGraphicsSettingManager
{
    private const string GfxUnPrefix = "un";
    private const string GfxState = "docked";

    public MyConfiguration Configuration { get; set; }

    public bool IsDocked { get; private set; }
    
    private string StatusTextInternal => IsDocked ? GfxState : $"{GfxUnPrefix}{GfxState}";
    public string StatusText => $"{StatusTextInternal.Substring(0,1).ToUpper()}{StatusTextInternal.Substring(1)}";

    private event EventHandler StatusChangedEvent;
    private readonly object _objectLock = new ();
    private readonly ILogger<GraphicsSettingManager> _logger;

    public GraphicsSettingManager(ILogger<GraphicsSettingManager> logger)
    {
        _logger = logger;
        Configuration = FileHelper.LoadSettings();
        IsDocked = Configuration.IsConfiguredAsDocked;
    }

    public event EventHandler OnStatusChanged
    {
        add
        {
            lock (_objectLock)
            {
                StatusChangedEvent += value;
            }
        }
        remove
        {
            lock (_objectLock)
            {
                StatusChangedEvent -= value;
            }
        }
    }

    public async Task Scan(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting scan...");
        while (!cancellationToken.IsCancellationRequested)
        {
            var gfxCount = GetGfxCount();
            var isDocked = gfxCount > 1;
            
            if (IsDocked != isDocked)
            {
                _logger.LogInformation($"Switching {GfxState} state...");
                if (isDocked)
                {
                    SetToDocked();
                }
                else
                {
                    SetToUndocked();
                }
                SaveConfig();
            }
           
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
        _logger.LogInformation("Scan Stopped...");
    }
    
    public void SetToDocked()
    {
        IsDocked = true;
        foreach(var game in Configuration.ConfiguredGames)
        {
            SwitchGameToDocked(game);
        }
        StatusChangedEvent?.Invoke(this, EventArgs.Empty);
    }
    
    public void SetToUndocked()
    {
        IsDocked = false;
        foreach(var game in Configuration.ConfiguredGames)
        {
            SwitchGameToUndocked(game);
        }
        StatusChangedEvent?.Invoke(this, EventArgs.Empty);
    }
    
    private static int GetGfxCount()
    {
        return new ManagementObjectSearcher("select * from Win32_VideoController").Get().Count;
    }

    private void SwitchGameToDocked(GameInfo gameInfo)
    {
        SwitchGame(gameInfo, isToDocked: true);
    }
    
    private void SwitchGameToUndocked(GameInfo gameInfo)
    {
        SwitchGame(gameInfo, isToDocked: false);
    }

    private void SwitchGame(GameInfo game, bool isToDocked)
    {
        var fromPrefix = isToDocked ? GfxUnPrefix : "";
        var toPrefix = isToDocked ? "" : GfxUnPrefix;

        var configFiles = game.SettingsFiles;
        foreach (var configFile in configFiles)
        {
            if (File.Exists(configFile))
            {
                File.Copy(configFile, $"{configFile}.{fromPrefix}{GfxState}", true);
            }

            if (File.Exists($"{configFile}.{toPrefix}{GfxState}"))
            {
                File.Copy($"{configFile}.{toPrefix}{GfxState}", configFile, true);
            }
        }
        

        _logger.LogInformation($"Switched {game.Name} to {toPrefix}{GfxState} mode.");
    }

    public void SaveConfig()
    {
        Configuration.IsConfiguredAsDocked = IsDocked;
        FileHelper.WriteSettingsFile(Configuration);
    }
}