using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Xml.Linq;
using GTweak.Core.Base;
using GTweak.Core.Item;
using GTweak.Core.Models;
using GTweak.Utilities.Controls;
using Ookii.Dialogs.Wpf;

namespace GTweak.Core.ViewModel
{
    internal class ToolsetViewModel : ViewModelBase
    {
        public ObservableCollection<ToolsetItem> Tools { get; } = new ObservableCollection<ToolsetItem>();

        public ICommand SelectFolderCommand { get; }
        public ICommand OpenFolderCommand { get; }

        public string DownloadPath
        {
            get => SettingsEngine.DownloadPath;
            set
            {
                if (SettingsEngine.DownloadPath != value)
                {
                    SettingsEngine.DownloadPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public ToolsetViewModel()
        {
            SelectFolderCommand = new RelayCommand(_ => SelectFolder());
            OpenFolderCommand = new RelayCommand(_ => OpenFolder());

            LoadApps();
        }

        private void LoadApps()
        {
            try
            {
                string xmlContent = Properties.Resources.AppsCatalog;

                if (!string.IsNullOrWhiteSpace(xmlContent))
                {
                    XDocument doc = XDocument.Parse(xmlContent);

                    foreach (var appElement in doc.Descendants("App"))
                    {
                        ToolsetModel model = new ToolsetModel(appElement);
                        Tools.Add(new ToolsetItem(model));
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
            }
        }

        private void SelectFolder()
        {
            try
            {
                VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog
                {
                    UseDescriptionForTitle = true,
                    SelectedPath = SettingsEngine.DownloadPath ?? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                };

                if (folderDialog.ShowDialog() == true)
                {
                    string selectedPath = folderDialog.SelectedPath;
                    if (!string.IsNullOrWhiteSpace(selectedPath) && Directory.Exists(selectedPath))
                    {
                        DownloadPath = selectedPath;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
            }
        }

        private void OpenFolder()
        {
            try
            {
                if (Directory.Exists(DownloadPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = DownloadPath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
            }
        }
    }
}