using System.Collections.Generic;
using System.Windows;
using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Modules.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal class ServicesViewModel : ViewModelPageBase<ServicesModel, ServicesTweaks>
    {
        public Visibility EdgeAvailable => AppxPackageHandler.IsEdgeInstalled ? Visibility.Visible : Visibility.Collapsed;

        protected override IReadOnlyDictionary<string, object> GetControlStates() => ServicesTweaks.ControlStates;

        protected override void Analyze(ServicesTweaks tweaks) => tweaks?.CheckAll();
    }
}
