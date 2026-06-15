using System.Collections.Generic;
using System.Windows;
using GTweak.Core.Base;

internal sealed class DataSystemModel : ViewModelBase
{
    private string _data;
    private string[] _dataItems;
    private bool _isEnabled;
    private int _blurValue;
    private Visibility _ipVisibility;
    private List<DataSystemModel> _items;

    public string Name { get; set; }

    public string Data
    {
        get => _data;
        set { _data = value; OnPropertyChanged(); }
    }

    public string[] DataItems
    {
        get => _dataItems;
        set { _dataItems = value; OnPropertyChanged(); }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(); }
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

    public List<DataSystemModel> Items
    {
        get => _items;
        set { _items = value; OnPropertyChanged(); }
    }
}