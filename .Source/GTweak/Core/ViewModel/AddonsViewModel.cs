using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GTweak.Core.Base;
using GTweak.Modules.Common;
using GTweak.Modules.Helpers;
using Ookii.Dialogs.Wpf;

namespace GTweak.Core.ViewModel
{
    internal class AddonsViewModel : ViewModelBase
    {
        public ObservableCollection<AddonModel> Addons { get; } = new ObservableCollection<AddonModel>();

        public ICommand SelectFolderCommand { get; }
        public ICommand UpdateCommand { get; }

        private bool _isRunAsTrustedInstaller;
        private readonly object _locker = new object();
        private string _lastKnownFolderPath = string.Empty;

        public bool IsRunAsTrustedInstaller
        {
            get => _isRunAsTrustedInstaller;
            set { _isRunAsTrustedInstaller = value; OnPropertyChanged(); }
        }

        public AddonsViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                SelectFolderCommand = new RelayCommand(_ => SelectFolder());
                UpdateCommand = new RelayCommand(_ => ScanFolder());

                if (!string.IsNullOrWhiteSpace(GlobalOptions.UserAddonsPath) && Directory.Exists(GlobalOptions.UserAddonsPath))
                {
                    ScanFolder();
                }
            }
        }

        private void SelectFolder()
        {
            try
            {
                VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog
                {
                    UseDescriptionForTitle = true,
                    SelectedPath = GlobalOptions.UserAddonsPath ?? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                };

                bool? dialogResult = folderDialog.ShowDialog();

                if (dialogResult != null && dialogResult.Value)
                {
                    string selectedPath = folderDialog.SelectedPath;
                    if (!string.IsNullOrWhiteSpace(selectedPath) && Directory.Exists(selectedPath))
                    {
                        GlobalOptions.UserAddonsPath = selectedPath;
                        ScanFolder();
                    }
                }
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }

        private void RunFile(AddonModel addon)
        {
            try
            {
                string fileName = string.Empty, arguments = string.Empty;

                switch (Path.GetExtension(addon.FilePath).ToLowerInvariant())
                {
                    case ".reg":
                        fileName = "reg.exe";
                        arguments = $"import \"{addon.FilePath}\"";
                        break;

                    case ".ps1":
                        fileName = PathTargets.Executable.PowerShell;
                        arguments = $"-ExecutionPolicy Bypass -File \"{addon.FilePath}\"";
                        break;
                    default:
                        fileName = PathTargets.Executable.CommandShell;
                        arguments = $"/k \"{addon.FilePath}\"";
                        break;
                }

                Task.Run(() => { CommandExecutor.RunProcessVisible(fileName, CommandExecutor.CleanCommand(arguments), addon.RequiresElevation); });
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }

        private void ScanFolder()
        {
            try
            {
                string path = GlobalOptions.UserAddonsPath;

                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                {
                    return;
                }

                if (_lastKnownFolderPath != path)
                {
                    lock (_locker)
                    {
                        Addons.Clear();
                    }

                    _lastKnownFolderPath = path;
                }

                IOrderedEnumerable<string> files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                                                         .Where(f => new[] { ".ps1", ".bat", ".cmd", ".reg" }.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                                                         .OrderBy(f => Path.GetFileName(f));

                List<string> currentFilePaths = Addons.Select(a => a.FilePath).ToList();

                bool collectionChanged = false;

                List<AddonModel> addonsToRemove = Addons.Where(a => !files.Contains(a.FilePath)).ToList();

                foreach (var addon in addonsToRemove)
                {
                    lock (_locker)
                    {
                        Addons.Remove(addon);
                    }
                    collectionChanged = true;
                }

                foreach (string file in files)
                {
                    if (currentFilePaths.Contains(file))
                    {
                        continue;
                    }

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
                        if (Application.Current != null && !string.IsNullOrEmpty(iconRes) && Application.Current.Resources.Contains(iconRes))
                        {
                            iconImage = Application.Current.Resources[iconRes] as ImageSource;
                        }
                    }
                    catch (Exception ex) { ErrorLogger.LogDebug(ex); }

                    iconImage ??= Application.Current.Resources["Img_BatFile"] as ImageSource;

                    RelayCommand runCommand = new RelayCommand(_ => RunFile(new AddonModel(file, Path.GetFileName(file), iconImage, null, IsRunAsTrustedInstaller)));
                    AddonModel addonItem = new AddonModel(file, Path.GetFileName(file), iconImage, runCommand, IsRunAsTrustedInstaller);

                    lock (_locker)
                    {
                        Addons.Add(addonItem);
                    }

                    collectionChanged = true;
                }

                if (collectionChanged)
                {
                    lock (_locker)
                    {
                        var sortedAddons = Addons.OrderBy(a => a.FileName).ToList();
                        Addons.Clear();
                        foreach (var addon in sortedAddons)
                        {
                            Addons.Add(addon);
                        }
                    }
                }
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }
    }
}