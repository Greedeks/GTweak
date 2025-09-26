using GTweak.Core.Model;
using GTweak.Utilities.Tweaks;
using System.Collections.Generic;

namespace GTweak.Core.ViewModel
{
    internal sealed class SystemVM : ViewModelPageBase<SystemModel, SystemTweaks>
    {
        protected override Dictionary<string, object> GetControlStates() => SystemTweaks.ControlStates;

        protected override void Analyze(SystemTweaks tweaks) => tweaks.AnalyzeAndUpdate();
    }
}
