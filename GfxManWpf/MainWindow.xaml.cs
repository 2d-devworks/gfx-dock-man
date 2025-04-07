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
    
    public string StatusText => _graphicsSettingManager.StatusText;
    
    public string TitleText => string.Format(App.TitleText, StatusText);

    public ICollection<GameInfo> ConfiguredGames
    {
        get => _graphicsSettingManager.Configuration.ConfiguredGames.OrderBy(n => n.Name).ToList();
        set => _graphicsSettingManager.Configuration.ConfiguredGames = value;
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
}