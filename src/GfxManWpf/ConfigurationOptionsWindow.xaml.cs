using System;
using System.Windows;
using GfxMan.Services.Model;

namespace GfxManWpf;

public partial class ConfigurationOptionsWindow
{
    public GfxConfigOption Model { get; }
    private readonly string _oldName;
    
    private readonly object _objectLock = new ();
    private event EventHandler OptionSavedEvent;

    public ConfigurationOptionsWindow(GfxConfigOption configOption)
    {
        Model = configOption;
        _oldName = configOption.Name;
        InitializeComponent();
    }
    
    public event EventHandler OnOptionSaved
    {
        add
        {
            lock (_objectLock)
            {
                OptionSavedEvent += value;
            }
        }
        remove
        {
            lock (_objectLock)
            {
                OptionSavedEvent -= value;
            }
        }
    }

    protected override void OnActivated(EventArgs e)
    {
        NameTextBox.Focus();
        NameTextBox.SelectAll();
    }

    private void SaveConfigurationButton_OnClick(object sender, RoutedEventArgs e)
    {
        OptionSavedEvent?.Invoke(this, new GfxConfigOptionEventArgs(Model, _oldName));
    }
}