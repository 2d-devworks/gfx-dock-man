using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using ListView = System.Windows.Controls.ListView;
using MessageBox = System.Windows.MessageBox;

namespace GfxManWpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INotifyPropertyChanged
{
    private readonly IGraphicsSettingManager _graphicsSettingManager;
    
    public string StatusText => _graphicsSettingManager.ActiveConfig;
    
    public string TitleText => string.Format(App.TitleText, StatusText);

    public ICollection<GameInfo> ConfiguredGames
    {
        get => _graphicsSettingManager.Configuration.ConfiguredGames.OrderBy(n => n.Name).ToList();
        set => _graphicsSettingManager.Configuration.ConfiguredGames = value;
    }

    public ICollection<GfxConfigOption> DisplayConfigurations
    {
        get => _graphicsSettingManager.Configuration.ConfigurationOptions;
        set => _graphicsSettingManager.Configuration.ConfigurationOptions = value;
    }

    public MainWindow(IGraphicsSettingManager graphicsSettingManager)
    {
        _graphicsSettingManager = graphicsSettingManager;
        _graphicsSettingManager.OnStatusChanged += (_,_) =>
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(TitleText));
            }));

        };
        Loaded += (_, _) =>
        {
            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);
        };
        InitializeComponent();
        Visibility = Visibility.Hidden;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private void GamesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListView { SelectedItem: GameInfo info }) return;
        
        CreateSettingsWindow(info);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void AddGameButton_OnClick(object sender, RoutedEventArgs e)
    {
        CreateSettingsWindow(new GameInfo());
    }

    private void CreateSettingsWindow(GameInfo game)
    {
        var settingsWindow = new GameSettingsWindow(game);
        settingsWindow.OnSettingsSaved += (_, e) =>
        {
            if (e is not GameInfoEventArgs { GameInfo: { } info }) return;
            SaveGameInfo(info);
        };
        settingsWindow.Show();
        settingsWindow.Focus();
    }

    private void SaveGameInfo(GameInfo game)
    {
        try
        {
            var item = _graphicsSettingManager.Configuration.ConfiguredGames.FirstOrDefault(n => n.Id == game.Id);
            if (item == null)
            {
                _graphicsSettingManager.Configuration.ConfiguredGames.Add(game);
            }

            _graphicsSettingManager.SaveConfig();
            OnPropertyChanged(nameof(ConfiguredGames));
            MessageBox.Show("Game settings have been saved.");
        }
        catch
        {
            MessageBox.Show("An error occurred while saving the settings.");
        }
    }

    private void ConfigurationsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListView { SelectedItem: GfxConfigOption option }) return;
        
        CreateGfxConfigurationWindow(option);
    }

    private void AddConfigurationButton_OnClick(object sender, RoutedEventArgs e)
    {
        var option = new GfxConfigOption()
        {
            Name = "NEW",
            PrimaryDisplayDriverName = _graphicsSettingManager.CurrentDisplayAdapter,
            PrimaryDisplayMaxHeight = _graphicsSettingManager.CurrentDisplayMaxHeight,
            PrimaryDisplayMaxWidth = _graphicsSettingManager.CurrentDisplayMaxWidth,
        };
        CreateGfxConfigurationWindow(option);
    }

    private void CreateGfxConfigurationWindow(GfxConfigOption configOption)
    {
        var configurationWindow = new ConfigurationOptionsWindow(configOption);
        configurationWindow.OnOptionSaved += (_, e) =>
        {
            if (e is not GfxConfigOptionEventArgs args) return;
            SaveConfigurationOption(configurationWindow, args);
        };
        configurationWindow.Show();
        configurationWindow.Focus();
    }

    private void SaveConfigurationOption(ConfigurationOptionsWindow window, GfxConfigOptionEventArgs args)
    {
        try
        {
            var item = DisplayConfigurations.FirstOrDefault(n => n.Name == args.ConfigOption.Name);
            
            if (item == null)
            {
                DisplayConfigurations.Add(args.ConfigOption);
                _graphicsSettingManager.SaveConfig();
            }
            
            if (DisplayConfigurations.Count(n => n.Name == args.ConfigOption.Name) > 1)
            {
                MessageBox.Show($"A configuration named {args.ConfigOption.Name} already exists.");
                window.Model.Name = args.OldName;
                return;
            }

            if (args.OldName != args.ConfigOption.Name)
            {
                _graphicsSettingManager.RenameConfigurationOption(args.OldName, args.ConfigOption.Name);
            }
            
            OnPropertyChanged(nameof(DisplayConfigurations));
            MessageBox.Show("Configuration option has been saved.");
        }
        catch
        {
            MessageBox.Show("An error occurred while saving the settings.");
        }
    }
}