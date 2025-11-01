﻿using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Maintenance;
using GTweak.Utilities.Tweaks;
using System.Collections.Generic;
using System.Windows;

namespace GTweak.Core.ViewModel
{
    internal class InterfaceViewModel : ViewModelPageBase<InterfaceModel, InterfaceTweaks>
    {
        public Visibility Win11FeatureOnly => HardwareData.OS.IsWin11 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility Win11FeatureAvailable => HardwareData.OS.IsWin11 && HardwareData.OS.Build.CompareTo(22621.2361m) >= 0 ? Visibility.Visible : Visibility.Collapsed;
        public bool IsBlockWithoutLicense => WindowsLicense.IsWindowsActivated;

        protected override Dictionary<string, object> GetControlStates() => InterfaceTweaks.ControlStates;

        protected override void Analyze(InterfaceTweaks tweaks) => tweaks.AnalyzeAndUpdate();
    }
}
