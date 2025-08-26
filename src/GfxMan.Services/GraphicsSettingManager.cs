using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.Logging;

namespace GfxMan.Services;

public class GraphicsSettingManager : HasOnStatusChangedEventServiceBase, IGraphicsSettingManager
{
    public GfxManConfiguration Configuration { get; set; }
    public string CurrentDisplayAdapter { get; private set; } = string.Empty;
    public uint CurrentDisplayMaxWidth => _primaryMonitorService.PrimaryMonitor.MaximumPixelWidth;
    public uint CurrentDisplayMaxHeight => _primaryMonitorService.PrimaryMonitor.MaximumPixelHeight;

    public string ActiveConfig { get; private set; }
    
    private readonly ILogger<GraphicsSettingManager> _logger;
    private readonly ISettingsFileService _settingsFileService;
    private readonly IPrimaryDisplayAdapterService _primaryDisplayAdapterService;
    private readonly IPrimaryMonitorService _primaryMonitorService;

    public GraphicsSettingManager(ILogger<GraphicsSettingManager> logger, ISettingsFileService settingsFileService, 
        IPrimaryDisplayAdapterService primaryDisplayAdapterService, IPrimaryMonitorService primaryMonitorService)
    {
        _logger = logger;
        _settingsFileService = settingsFileService;
        _primaryDisplayAdapterService = primaryDisplayAdapterService;
        _primaryMonitorService = primaryMonitorService;
        Configuration = settingsFileService.LoadConfiguration();
        ActiveConfig = Configuration.ActiveConfiguration;
    }
    
    private void OnPrimaryDisplayAdapterOrMonitorChanged(object sender, EventArgs e)
    {
        CheckCurrentState();
    }

    public async Task WatchForChanges(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Watching for changes...");
        
        _primaryDisplayAdapterService.OnStatusChanged += OnPrimaryDisplayAdapterOrMonitorChanged;
        _primaryMonitorService.OnStatusChanged += OnPrimaryDisplayAdapterOrMonitorChanged;
        
        CheckCurrentState();
        await Task.Run(() => _primaryDisplayAdapterService.WatchForChanges(cancellationToken), cancellationToken);
        
        _primaryDisplayAdapterService.OnStatusChanged -= OnPrimaryDisplayAdapterOrMonitorChanged;
        _primaryMonitorService.OnStatusChanged -= OnPrimaryDisplayAdapterOrMonitorChanged;
        
        _logger.LogInformation("Stopped watching...");
    }

    private void CheckCurrentState()
    {
        var gfxState = GetGfxState();

        if (ActiveConfig == gfxState)
        {
            return;
        }

        ChangeActiveConfiguration(gfxState);
        SaveConfig();
    }
    
    public void ChangeActiveConfiguration(string newActiveConfiguration)
    {
        if (Configuration.ActiveConfiguration == newActiveConfiguration || Configuration.ConfigurationOptions.All(n => n.Name != newActiveConfiguration))
        {
            return;
        }
        
        _logger.LogInformation($"Switching from {ActiveConfig} to {newActiveConfiguration}...");
        
        foreach(var game in Configuration.ConfiguredGames)
        {
            SwitchGame(game, newActiveConfiguration, ActiveConfig);
        }
        
        ActiveConfig = newActiveConfiguration;
        InvokeStatusChangedEvent(EventArgs.Empty);
    }

    private string GetGfxState()
    {
        var defaultGfxConfig = Configuration.ConfigurationOptions.First();
        var activeDisplayAdapter = _primaryDisplayAdapterService.PrimaryDisplayAdapter?.Name ?? 
                     defaultGfxConfig.PrimaryDisplayDriverName;
        
        CurrentDisplayAdapter = activeDisplayAdapter;

        var config = Configuration.ConfigurationOptions
            .FirstOrDefault(n => n.PrimaryDisplayDriverName == activeDisplayAdapter &&
                                 n.PrimaryDisplayMaxWidth == CurrentDisplayMaxWidth &&
                                 n.PrimaryDisplayMaxHeight == CurrentDisplayMaxHeight)?.Name ?? 
                     Configuration.ConfigurationOptions
                         .FirstOrDefault(n => n.PrimaryDisplayDriverName == activeDisplayAdapter)?.Name ??
                     ActiveConfig;

        return config;
    }

    private void SwitchGame(GameInfo game, string toConfig, string fromConfig)
    {
        try
        {
            _settingsFileService.SwitchGameConfiguration(game, fromConfig, toConfig);
            _logger.LogInformation($"Switched {game.Name} to {toConfig} mode.");
        }
        catch(Exception ex)
        {
            _logger.LogError($"Could not switch {game.Name} to {toConfig} mode. {ex.Message}");
        }
    }
    
    public void RenameConfigurationOption(string fromConfig, string toConfig)
    {
        _settingsFileService.RenameAllGameConfigurationOptionFiles(Configuration, fromConfig, toConfig);

        if (ActiveConfig != fromConfig)
        {
            return;
        }
        
        ActiveConfig = toConfig;
        SaveConfig();
        
        InvokeStatusChangedEvent(EventArgs.Empty);
    }

    public void SaveConfig()
    {
        Configuration.ActiveConfiguration = ActiveConfig;
        _settingsFileService.SaveConfiguration(Configuration);
    }
}