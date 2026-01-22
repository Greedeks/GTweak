using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GTweak.Core.Base;
using GTweak.Utilities.Controls;
using Ookii.Dialogs.Wpf;

namespace GTweak.Core.ViewModel
{
    internal class AddonsViewModel : ViewModelBase
    {
        internal class AddonItem
        {
            public string FilePath { get; }
            public string FileName { get; }
            public ImageSource IconImage { get; }
            public ICommand RunCommand { get; }
            private readonly Action _onRunComplete;
            public bool RequiresElevation { get; }

            public AddonItem(string filePath, string fileName, ImageSource iconImage, Action onRunComplete, bool requiresElevation = false)
            {
                FilePath = filePath;
                FileName = fileName;
                IconImage = iconImage;
                _onRunComplete = onRunComplete;
                RequiresElevation = requiresElevation;

                RunCommand = new RelayCommand(async _ => await RunFileAsync());
            }

            private async Task RunFileAsync()
            {
                try
                {
                    var ext = Path.GetExtension(FilePath).ToLowerInvariant();

                    if (RequiresElevation)
                    {
                        
                    }
                    else
                    {
         
                    }

                    _onRunComplete?.Invoke();
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
        }

        private static readonly string[] AllowedExtensions = new[] { ".ps1", ".bat", ".cmd", ".reg" };

        public ObservableCollection<AddonItem> Addons { get; } = new ObservableCollection<AddonItem>();

        public ICommand SelectFolderCommand { get; }
        public ICommand UpdateCommand { get; }

        private bool _isRunAsTrustedInstaller;

        public bool IsRunAsTrustedInstaller
        {
            get => _isRunAsTrustedInstaller;
            set
            {
                _isRunAsTrustedInstaller = value;
                OnPropertyChanged();
            }
        }

        public AddonsViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                SelectFolderCommand = new RelayCommand(_ => SelectFolder());
                UpdateCommand = new RelayCommand(_ => ScanFolder());

                if (!string.IsNullOrWhiteSpace(SettingsEngine.UserAddonsPath) && Directory.Exists(SettingsEngine.UserAddonsPath))
                {
                    ScanFolder();
                }
            }
        }

        private void SelectFolder()
        {
            try
            {
                var folderDialog = new VistaFolderBrowserDialog();
                if (folderDialog.ShowDialog() == false)
                {
                    return;
                }

                string selectedPath = folderDialog.SelectedPath;
                if (!string.IsNullOrWhiteSpace(selectedPath) && Directory.Exists(selectedPath))
                {
                    SettingsEngine.UserAddonsPath = selectedPath;
                    ScanFolder();
                }
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        private void ScanFolder()
        {
            Addons.Clear();

            try
            {
                string path = SettingsEngine.UserAddonsPath;

                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                {
                    return;
                }

                IOrderedEnumerable<string> files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                                     .Where(f => AllowedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                                     .OrderBy(f => Path.GetFileName(f));

                foreach (string file in files)
                {
                    string iconRes = Path.GetExtension(file).ToLowerInvariant() switch
                    {
                        ".ps1" => "Img_PowershellFile",
                        ".reg" => "Img_RegistryFile",
                        ".bat" => "Img_BatFile",
                        ".cmd" => "Img_CmdFile",
                        _ => "Img_BatFile"
                    };

                    ImageSource iconImage = null;
                    try
                    {
                        if (Application.Current != null)
                        {
                            if (Application.Current.Resources.Contains(iconRes))
                            {
                                iconImage = Application.Current.Resources[iconRes] as ImageSource;
                            }
                        }
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }

                    AddonItem addonItem = new AddonItem(file, Path.GetFileName(file), iconImage, () => ScanFolder(), IsRunAsTrustedInstaller);
                    Addons.Add(addonItem);
                }
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }
    }
}
