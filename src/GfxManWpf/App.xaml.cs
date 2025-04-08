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
    private const string MainIconResourceUri = "pack://application:,,,/GfxManWpf;component/Assets/handheld.ico";
    
    private ServiceProvider _serviceProvider;
    private IGraphicsSettingManager _graphicsSettingManager;

    public const string TitleText = "Graphics Dock Manager [{0}]";
    public static ImageSource MainIconImageSource => new ImageSourceConverter().ConvertFromString(MainIconResourceUri) as ImageSource;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        var tokenSource = new CancellationTokenSource();
        services.AddLogging(config =>
        {
            config.AddConsole();
        });
        services.AddSingleton(tokenSource);
        services.AddGfxManServices();
        services.AddSingleton<MainWindow>();
        
        _serviceProvider = services.BuildServiceProvider();
        _graphicsSettingManager = _serviceProvider.GetService<IGraphicsSettingManager>();
        
        var window = _serviceProvider.GetService<MainWindow>();
        SetupTrayIcon(window, tokenSource);
        
        Task.Run(async () =>
        {
            await _graphicsSettingManager.Scan(tokenSource.Token);
        }, tokenSource.Token);
    }
    
    private void SetupTrayIcon(MainWindow window, CancellationTokenSource tokenSource)
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
        trayIcon.ContextMenu = GetContextMenu(window, tokenSource);
        _graphicsSettingManager.OnStatusChanged += (_,_) =>
        {
            trayIcon.Text = string.Format(TitleText, _graphicsSettingManager.ActiveConfig);
        };
    }

    private ContextMenu GetContextMenu(MainWindow window, CancellationTokenSource tokenSource)
    {
        var menu = new ContextMenu();
        
        var statusItem = new MenuItem();
        statusItem.Text = _graphicsSettingManager.ActiveConfig;

        _graphicsSettingManager.OnStatusChanged += (_,_) =>
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                statusItem.Text = _graphicsSettingManager.ActiveConfig;
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
        quitItem.Click += (_,_) =>
        {
            tokenSource.Cancel();
            Shutdown();
        };
        
        menu.MenuItems.Add(statusItem);
        menu.MenuItems.Add(new MenuItem("-"));
        menu.MenuItems.Add(settingsItem);
        menu.MenuItems.Add(new MenuItem("-"));
        menu.MenuItems.Add(quitItem);
        
        return menu;
    }
}