using System.Windows;
using GTweak.Core.Base;

internal sealed class DataSystemModel : ViewModelBase
{
    private string _data;
    private int _blurValue;
    private Visibility _ipVisibility;

    public string Name { get; set; }

    public string Data
    {
        get => _data;
        set { _data = value; OnPropertyChanged(); }
    }

    public int BlurValue
    {
        get => _blurValue;
        set { _blurValue = value; OnPropertyChanged(); }
    }

    public Visibility IpVisibility
    {
        get => _ipVisibility;
        set { _ipVisibility = value; OnPropertyChanged(); }
    }
}