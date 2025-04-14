using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GfxMan.Services.Model;

public class GfxConfigOption: INotifyPropertyChanged
{
    public const string DefaultConfigName = "Default";
    public const string DefaultDisplayDriverName = "AMD Radeon (TM) 780M Graphics";

    private string _name = DefaultConfigName;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }
    public string PrimaryDisplayDriverName { get; set; } = DefaultDisplayDriverName;
    
    public uint PrimaryDisplayMaxWidth { get; set; } = 1920;
    
    public uint PrimaryDisplayMaxHeight { get; set; } = 1080;
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}