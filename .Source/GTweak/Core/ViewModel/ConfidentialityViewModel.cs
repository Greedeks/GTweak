using System.Collections.Generic;
using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Utilities.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal class ConfidentialityViewModel : ViewModelPageBase<ConfidentialityModel, ConfidentialityTweaks>
    {
        protected override Dictionary<string, object> GetControlStates() => ConfidentialityTweaks.ControlStates;

        protected override void Analyze(ConfidentialityTweaks tweaks) => tweaks?.AnalyzeAndUpdate();
    }
}
