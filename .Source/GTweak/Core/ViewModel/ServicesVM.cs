using GTweak.Core.Model;
using GTweak.Utilities.Tweaks;
using System.Collections.Generic;

namespace GTweak.Core.ViewModel
{
    internal class ServicesVM : BasePageVM<ServicesModel, ServicesTweaks>
    {
        protected override Dictionary<string, object> GetControlStates() => ServicesTweaks.ControlStates;

        protected override void Analyze(ServicesTweaks tweaks) => tweaks.AnalyzeAndUpdate();
    }
}
