using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
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
    private ServiceProvider _serviceProvider;
    private IGraphicsSettingManager _graphicsSettingManager;

    public const string TitleText = "Graphics Dock Manager [{0}]";

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        var tokenSource = new CancellationTokenSource();
        services.AddLogging(config =>
        {
            config.AddConsole();
        });
        services.AddSingleton(tokenSource);
        services.AddSingleton<IGraphicsSettingManager, GraphicsSettingManager>();
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
        trayIcon.Text = string.Format(TitleText, _graphicsSettingManager.StatusText);
        trayIcon.Icon = new Icon("handheld.ico");
        trayIcon.Visible = true;
        trayIcon.MouseDoubleClick += (_,_) =>
        {
            window.Show();
            window.Focus();
        };
        trayIcon.ContextMenu = GetContextMenu(window, tokenSource);
        _graphicsSettingManager.OnStatusChanged += (_,_) =>
        {
            trayIcon.Text = string.Format(TitleText, _graphicsSettingManager.StatusText);
        };
    }

    private ContextMenu GetContextMenu(MainWindow window, CancellationTokenSource tokenSource)
    {
        var menu = new ContextMenu();
        
        var statusItem = new MenuItem();
        statusItem.Text = _graphicsSettingManager.StatusText;

        _graphicsSettingManager.OnStatusChanged += (_,_) =>
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                statusItem.Text = _graphicsSettingManager.StatusText;
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