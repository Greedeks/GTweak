using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Tweaks;
using System.Collections.Generic;
using System.Windows;

namespace GTweak.Core.ViewModel
{
    internal class SystemViewModel : ViewModelPageBase<SystemModel, SystemTweaks>
    {
        public Visibility RealtekSupportAvailable => HardwareData.VendorDetection.Realtek ? Visibility.Visible : Visibility.Collapsed;

        protected override Dictionary<string, object> GetControlStates() => SystemTweaks.ControlStates;

        protected override void Analyze(SystemTweaks tweaks) => tweaks.AnalyzeAndUpdate();
    }
}
