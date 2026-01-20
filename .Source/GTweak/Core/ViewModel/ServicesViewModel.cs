using System.Collections.Generic;
using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Utilities.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal class ServicesViewModel : ViewModelPageBase<ServicesModel, ServicesTweaks>
    {
        protected override Dictionary<string, object> GetControlStates() => ServicesTweaks.ControlStates;

        protected override void Analyze(ServicesTweaks tweaks) => tweaks?.AnalyzeAndUpdate();
    }
}
