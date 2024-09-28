﻿using GTweak.Core.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GTweak.Core
{
    internal class ViewModelBase : MainModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string _propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_propName));
        }
    }
}
