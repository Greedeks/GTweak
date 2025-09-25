using GTweak.Core.Model;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Tweaks;
using System.Collections.Generic;

namespace GTweak.Core.ViewModel
{
    internal class InterfaceVM : BasePageVM<InterfaceModel, InterfaceTweaks>
    {
        public bool IsBlockForWin10 => SystemDiagnostics.IsWindowsVersion[11];
        public bool IsBlockWithoutLicense => WindowsLicense.IsWindowsActivated;

        protected override Dictionary<string, object> GetControlStates() => InterfaceTweaks.ControlStates;

        protected override void Analyze(InterfaceTweaks tweaks) => tweaks.AnalyzeAndUpdate();
    }
}
