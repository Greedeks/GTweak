using System.Collections.Generic;
using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Modules.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal sealed class ConfidentialityViewModel : ViewModelPageBase<ConfidentialityModel, ConfidentialityTweaks>
    {
        protected override IReadOnlyDictionary<string, object> Analyze(ConfidentialityTweaks tweaks) => tweaks.CheckAll();
    }
}
