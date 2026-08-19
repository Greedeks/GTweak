using System.Collections.Generic;
using System.Windows;
using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Modules.Configuration;
using GTweak.Modules.Managers;
using GTweak.Modules.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal class SystemViewModel : ViewModelPageBase<SystemModel, SystemTweaks>
    {
        public Visibility RealtekSupport => HardwareData.VendorDetection.Realtek ? Visibility.Visible : Visibility.Collapsed;
        public Visibility BluetoothSupport => BluetoothManager.IsAvailable ? Visibility.Visible : Visibility.Collapsed;

        protected override IReadOnlyDictionary<string, object> GetControlStates() => SystemTweaks.ControlStates;

        protected override void Analyze(SystemTweaks tweaks) => tweaks?.CheckAll();
    }
}
