using System.Collections.Generic;
using System.Windows;
using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Modules.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal sealed class ServicesViewModel : ViewModelPageBase<ServicesModel, ServicesTweaks>
    {
        public Visibility EdgeAvailable => AppxPackageHandler.IsEdgeInstalled ? Visibility.Visible : Visibility.Collapsed;

        protected override IReadOnlyDictionary<string, object> Analyze(ServicesTweaks tweaks) => tweaks.CheckAll();
    }
}
