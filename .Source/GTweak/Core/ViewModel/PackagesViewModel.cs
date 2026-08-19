using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Modules.Configuration;
using GTweak.Modules.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal class PackagesViewModel : ViewModelBase
    {
        public ObservableCollection<PackagesModel> DisplayState { get; set; }

        public Visibility Win11FeatureOnly => HardwareData.OS.IsWin11 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility Win10FeatureOnly => HardwareData.OS.IsWin10 ? Visibility.Visible : Visibility.Collapsed;

        public PackagesModel this[string name] => DisplayState.FirstOrDefault(d => d.Name == name);

        public PackagesViewModel()
        {
            DisplayState = new ObservableCollection<PackagesModel>();

            BuildCollection();

            AppxPackageHandler.DataChanged += delegate
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (PackagesModel item in DisplayState)
                    {
                        UpdatePackageState(item);
                    }
                }), DispatcherPriority.ApplicationIdle);

            };
        }

        private void BuildCollection()
        {
            DisplayState.Clear();

            foreach (var kv in AppxPackageHandler.PackagesDetails)
            {
                string name = kv.Key;
                AppxPackageHandler.PackagesInfo details = kv.Value;

                PackagesModel pkg = new PackagesModel
                {
                    Name = name,
                    IsUnavailable = !details.IsUnavailable
                };

                UpdatePackageState(pkg);
                DisplayState.Add(pkg);
            }
        }

        private void UpdatePackageState(PackagesModel item)
        {
            if (item != null && !string.IsNullOrWhiteSpace(item.Name) && AppxPackageHandler.PackagesDetails?.TryGetValue(item?.Name, out var val) == true && val != null)
            {
                item.IsUnavailable = !val.IsUnavailable;

                switch (item?.Name.ToLowerInvariant())
                {
                    case "onedrive":
                        item.Installed = AppxPackageHandler.IsOneDriveInstalled;
                        return;
                    case "edge":
                        item.Installed = AppxPackageHandler.IsEdgeInstalled;
                        return;
                    default:
                        break;
                }

                IReadOnlyList<string> scripts = val.Scripts;

                if (scripts != null && scripts?.Count > 0)
                {
                    item.Installed = scripts.Any(pattern => AppxPackageHandler.InstalledPackagesCache.Any(pkg => Regex.IsMatch(pkg, $"^{Regex.Escape(pattern)}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)));
                }
                else
                {
                    item.Installed = false;
                }
            }
        }
    }
}
