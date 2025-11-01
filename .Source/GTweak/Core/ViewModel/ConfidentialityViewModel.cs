﻿using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Tweaks;
using System.Collections.Generic;
using System.Windows;

namespace GTweak.Core.ViewModel
{
    internal class ConfidentialityViewModel : ViewModelPageBase<ConfidentialityModel, ConfidentialityTweaks>
    {
        public Visibility NvSupportAvailable => HardwareData.VendorDetection.Nvidia ? Visibility.Visible : Visibility.Collapsed;

        protected override Dictionary<string, object> GetControlStates() => ConfidentialityTweaks.ControlStates;

        protected override void Analyze(ConfidentialityTweaks tweaks) => tweaks.AnalyzeAndUpdate();
    }
}
