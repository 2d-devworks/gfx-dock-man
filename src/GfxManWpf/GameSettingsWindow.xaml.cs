using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using GfxMan.Services.Model;
using Button = System.Windows.Controls.Button;

namespace GfxManWpf;

public partial class GameSettingsWindow
{
    public GameInfo Model { get; }
    public BindingList<string> MySettingsFiles { get; }

    private readonly object _objectLock = new ();
    private event EventHandler SettingsSavedEvent;
    
    public GameSettingsWindow(GameInfo gameInfo)
    {
        Model = gameInfo;
        MySettingsFiles = new BindingList<string>(gameInfo.SettingsFiles);
        InitializeComponent();
    }
    
    public event EventHandler OnSettingsSaved
    {
        add
        {
            lock (_objectLock)
            {
                SettingsSavedEvent += value;
            }
        }
        remove
        {
            lock (_objectLock)
            {
                SettingsSavedEvent -= value;
            }
        }
    }

    private void SaveGameSettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        SettingsSavedEvent?.Invoke(this, new GameInfoEventArgs(Model));
    }
    
    

    private void AddSettingsFileButton_OnClick(object sender, RoutedEventArgs e)
    {
        var filePath = GetFilePath();
        if (filePath == null)
        {
            return;
        }
        MySettingsFiles.Add(filePath);
    }

    private void ChangeSettingsFile_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        
        var filePath = GetFilePath();
        if (filePath == null)
        {
            return;
        }
        MySettingsFiles.Remove(button.DataContext as string);
        MySettingsFiles.Add(filePath);
    }
    
    private string GetFilePath()
    {
        var fileDialog = new OpenFileDialog();
        if (fileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return null;
        }
        
        var filePath = fileDialog.FileName;
        return filePath;

    }

    private void DeleteSettingsFile_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        MySettingsFiles.Remove(button.DataContext as string);
    }

    protected override void OnActivated(EventArgs e)
    {
        NameTextBox.Focus();
    }
}