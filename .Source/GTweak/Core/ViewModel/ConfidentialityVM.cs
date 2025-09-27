using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Utilities.Tweaks;
using System.Collections.Generic;

namespace GTweak.Core.ViewModel
{
    internal class ConfidentialityVM : ViewModelPageBase<ConfidentialityModel, ConfidentialityTweaks>
    {
        protected override Dictionary<string, object> GetControlStates() => ConfidentialityTweaks.ControlStates;

        protected override void Analyze(ConfidentialityTweaks tweaks) => tweaks.AnalyzeAndUpdate();
    }

}
