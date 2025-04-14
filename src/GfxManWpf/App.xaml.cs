using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using GfxMan.Services;
using GfxMan.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GfxManWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private const string MainIconResourceUri = "pack://application:,,,/GfxManWpf;component/Assets/2d-devworks.ico";
    public const string TitleText = "Gfx Settings Manager [{0}]";

    public bool ShouldWatchForChanges
    {
        get => _graphicsSettingManager.Configuration.ShouldWatchForChanges; 
        set => _graphicsSettingManager.Configuration.ShouldWatchForChanges = value;
    }
    
    private ServiceProvider _serviceProvider;
    private IGraphicsSettingManager _graphicsSettingManager;
    
    public static ImageSource MainIconImageSource => new ImageSourceConverter().ConvertFromString(MainIconResourceUri) as ImageSource;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        var tokenSource = new CancellationTokenSource();
        services.AddLogging(config =>
        {
            config.AddConsole();
        });
        services.AddGfxManServices();
        services.AddSingleton<MainWindow>();
        
        _serviceProvider = services.BuildServiceProvider();
        _graphicsSettingManager = _serviceProvider.GetService<IGraphicsSettingManager>();
        
        var window = _serviceProvider.GetService<MainWindow>();
        SetupTrayIcon(window, tokenSource, _serviceProvider.GetService<ILogger<App>>());

        if (!_graphicsSettingManager.Configuration.ShouldWatchForChanges)
        {
            return;
        }
        
        Task.Run(async () =>
        {
            await _graphicsSettingManager.WatchForChanges(tokenSource.Token);
        }, tokenSource.Token);
    }
    
    private void SetupTrayIcon(MainWindow window, CancellationTokenSource tokenSource, ILogger<App> logger)
    {
        var trayIcon = new NotifyIcon();
        trayIcon.Text = string.Format(TitleText, _graphicsSettingManager.ActiveConfig);
        using (var iconStream =
               GetResourceStream(new Uri(MainIconResourceUri))!.Stream)
        {
            trayIcon.Icon = new Icon(iconStream);
        }
        
        trayIcon.Visible = true;
        trayIcon.MouseDoubleClick += (_,_) =>
        {
            window.Show();
            window.Focus();
        };
        trayIcon.ContextMenu = GetContextMenu(window, tokenSource, logger);
        _graphicsSettingManager.OnStatusChanged += (_,_) =>
        {
            trayIcon.Text = string.Format(TitleText, _graphicsSettingManager.ActiveConfig);
        };
    }

    private ContextMenu GetContextMenu(MainWindow window, CancellationTokenSource tokenSource, ILogger<App> logger)
    {
        var menu = new ContextMenu();
        
        var statusItem = new MenuItem()
        {
            Text = _graphicsSettingManager.ActiveConfig
        };
        
        var configurationsItem = GetConfigurationsItem();
        
        _graphicsSettingManager.OnStatusChanged += (_,_) =>
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                statusItem.Text = _graphicsSettingManager.ActiveConfig;
                foreach (MenuItem item in configurationsItem.MenuItems)
                {
                    item.Checked = item.Text == _graphicsSettingManager.ActiveConfig;
                }
            }));

        };
        
        var settingsItem = new MenuItem("Settings");
        settingsItem.Click += (_,_) =>
        {
            window.Show();
            window.Focus();
        };
        
        var quitItem = new MenuItem();
        quitItem.Text = "Exit";
        quitItem.Click += async (_,_) =>
        {
            logger.LogInformation("Shutting down...");
            tokenSource.Cancel();
            await Task.Delay(TimeSpan.FromSeconds(5));
            Shutdown();
        };
        
        menu.MenuItems.Add(statusItem);
        menu.MenuItems.Add(configurationsItem);
        menu.MenuItems.Add(new MenuItem("-"));
        menu.MenuItems.Add(GetWatcherMenuItem(tokenSource));
        menu.MenuItems.Add(new MenuItem("-"));
        menu.MenuItems.Add(settingsItem);
        menu.MenuItems.Add(new MenuItem("-"));
        menu.MenuItems.Add(quitItem);
        
        return menu;
    }

    private MenuItem GetConfigurationsItem()
    {
        var configurationsItem = new MenuItem()
        {
            Text = "Configurations"
        };
        foreach (var option in _graphicsSettingManager.Configuration.ConfigurationOptions)
        {
            var subItem = new MenuItem()
            {
                Text = option.Name,
                Checked = option.Name == _graphicsSettingManager.ActiveConfig,
            };
            subItem.Click += (_, _) =>
            {
                _graphicsSettingManager.ChangeActiveConfiguration(subItem.Text);
                _graphicsSettingManager.SaveConfig();
            };
            configurationsItem.MenuItems.Add(subItem);
        }
        return configurationsItem;
    }

    private MenuItem GetWatcherMenuItem(CancellationTokenSource tokenSource)
    {
        var watcherItem = new MenuItem()
        {
            Text = "Auto-swapping",
            Checked = ShouldWatchForChanges,
        };
        watcherItem.Click += async (_, _) =>
        {
            ShouldWatchForChanges = !ShouldWatchForChanges;
            watcherItem.Checked = ShouldWatchForChanges;
            _graphicsSettingManager.SaveConfig();
            
            if (ShouldWatchForChanges)
            {
                tokenSource = new CancellationTokenSource();
                await _graphicsSettingManager.WatchForChanges(tokenSource.Token);
                return;
            }
            
            tokenSource.Cancel();
        };
        
        return watcherItem;
    }
}