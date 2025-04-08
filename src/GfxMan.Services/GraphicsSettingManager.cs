using System.IO.Abstractions;
using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.Logging;

namespace GfxMan.Services;

public class GraphicsSettingManager : IGraphicsSettingManager
{
    public GfxManConfiguration Configuration { get; set; }

    public string ActiveConfig { get; private set; }
    
    private event EventHandler StatusChangedEvent;
    private readonly object _objectLock = new ();
    
    private readonly ILogger<GraphicsSettingManager> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly ISettingsFileService _settingsFileService;
    private readonly IDisplayAdapterService _displayAdapterService;

    public GraphicsSettingManager(ILogger<GraphicsSettingManager> logger, IFileSystem fileSystem, ISettingsFileService settingsFileService, IDisplayAdapterService displayAdapterService)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _settingsFileService = settingsFileService;
        _displayAdapterService = displayAdapterService;
        Configuration = settingsFileService.LoadConfiguration();
        ActiveConfig = Configuration.ActiveConfiguration;
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
        _displayAdapterService.OnStatusChanged += (_, _) =>
        {
            var gfxState = GetGfxState();

            if (ActiveConfig == gfxState)
            {
                return;
            }

            _logger.LogInformation($"Switching from {ActiveConfig} to {gfxState}...");
            ChangeActiveConfiguration(gfxState);
            SaveConfig();
        };
        
        await Task.Run(() => _displayAdapterService.WatchForChanges(cancellationToken), cancellationToken);
        _logger.LogInformation("Scan Stopped...");
    }

    public void ChangeActiveConfiguration(string newActiveConfiguration)
    {
        foreach(var game in Configuration.ConfiguredGames)
        {
            SwitchGame(game, newActiveConfiguration, ActiveConfig);
        }
        ActiveConfig = newActiveConfiguration;
        StatusChangedEvent?.Invoke(this, EventArgs.Empty);
    }
    
    private int GetGfxCount()
    {
        return _displayAdapterService.DisplayAdapters.Values.Count(isOk => isOk);
    }

    private string GetGfxState()
    {
        var defaultGfxConfig = Configuration.ConfigurationOptions.First();
        
        if (GetGfxCount() == 1)
        {
            return defaultGfxConfig.Name;
        }
        
        var driver = _displayAdapterService.DisplayAdapters.FirstOrDefault(n => n.Value && n.Key != defaultGfxConfig.Name).Key ?? defaultGfxConfig.PrimaryDisplayDriverName;
        return Configuration.ConfigurationOptions.FirstOrDefault(n => n.PrimaryDisplayDriverName == driver)?.Name;
    }

    private void SwitchGame(GameInfo game, string toConfig, string fromConfig)
    {
        var configFiles = game.SettingsFiles;
        foreach (var configFile in configFiles)
        {
            if (string.IsNullOrEmpty(configFile))
            {
                continue;
            }
            
            if (_fileSystem.File.Exists(configFile))
            {
                _fileSystem.File.Copy(configFile, fromConfig, true);
            }
        
            if (_fileSystem.File.Exists($"{configFile}.{toConfig}"))
            {
                _fileSystem.File.Copy($"{configFile}.{toConfig}", configFile, true);
            }
        }

        _logger.LogInformation($"Switched {game.Name} to {toConfig} mode.");
    }

    public void SaveConfig()
    {
        Configuration.ActiveConfiguration = ActiveConfig;
        _settingsFileService.SaveConfiguration(Configuration);
    }
}