using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using GTweak.Core.Base;
using GTweak.Core.Model;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Tweaks;

namespace GTweak.Core.ViewModel
{
    internal class PakagesViewModel : ViewModelBase
    {
        public ObservableCollection<PakagesModel> DisplayState { get; set; }

        public Visibility Win11FeatureOnly => HardwareData.OS.IsWin11 ? Visibility.Visible : Visibility.Collapsed;

        public PakagesModel this[string name] => DisplayState.FirstOrDefault(d => d.Name == name);

        public PakagesViewModel()
        {
            DisplayState = new ObservableCollection<PakagesModel>();

            BuildCollection();

            UninstallingPakages.DataChanged += delegate
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (PakagesModel item in DisplayState)
                    {
                        UpdatePackageState(item);
                    }
                }), DispatcherPriority.ApplicationIdle);

            };
        }

        private void BuildCollection()
        {
            DisplayState.Clear();

            foreach (var kv in UninstallingPakages.PackagesDetails)
            {
                string name = kv.Key;
                UninstallingPakages.PackagesInfo details = kv.Value;

                PakagesModel pkg = new PakagesModel
                {
                    Name = name,
                    IsUnavailable = !details.IsUnavailable
                };

                UpdatePackageState(pkg);
                DisplayState.Add(pkg);
            }
        }

        private void UpdatePackageState(PakagesModel item)
        {
            if (item != null && !string.IsNullOrWhiteSpace(item.Name) && UninstallingPakages.PackagesDetails?.TryGetValue(item?.Name, out var val) == true && val != null)
            {
                item.IsUnavailable = !val.IsUnavailable;

                switch (item?.Name.ToLowerInvariant())
                {
                    case "onedrive":
                        item.Installed = UninstallingPakages.IsOneDriveInstalled;
                        return;

                    case "edge":
                        item.Installed = UninstallingPakages.IsEdgeInstalled;
                        return;
                }

                IReadOnlyList<string> scripts = val.Scripts;

                if (scripts != null && scripts?.Count > 0)
                {
                    item.Installed = scripts.Any(pattern => UninstallingPakages.InstalledPackagesCache.Any(pkg => Regex.IsMatch(pkg, $"^{Regex.Escape(pattern)}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)));
                }
                else
                {
                    item.Installed = false;
                }
            }
        }
    }
}
