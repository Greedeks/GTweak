using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Data;
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

        private string _searchText;

        public ICollectionView ToolsView { get; }

        public ICommand SelectFolderCommand { get; }
        public ICommand OpenFolderCommand { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    ToolsView.Refresh();
                }
            }
        }

        public ToolsetViewModel()
        {
            SelectFolderCommand = new RelayCommand(_ => SelectFolder());
            OpenFolderCommand = new RelayCommand(_ => OpenFolder());

            LoadApps();

            ToolsView = CollectionViewSource.GetDefaultView(Tools);
            ToolsView.Filter = FilterTools;
        }

        private void LoadApps()
        {
            try
            {
                string xmlContent = Properties.Resources.AppsCatalog;

                if (!string.IsNullOrWhiteSpace(xmlContent))
                {
                    XDocument doc = XDocument.Parse(xmlContent);

                    foreach (XElement appElement in doc.Descendants("App"))
                    {
                        ToolsetModel model = new ToolsetModel(appElement);
                        Tools.Add(new ToolsetItem(model));
                    }
                }
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        private bool FilterTools(object obj)
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                if (obj is ToolsetItem item)
                {
                    bool nameMatch = item.AppName?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool authorMatch = item.AuthorName?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;

                    return nameMatch || authorMatch;
                }

                return false;
            }

            return true;
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
                        SettingsEngine.DownloadPath = selectedPath;
                    }
                }
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        private void OpenFolder()
        {
            try
            {
                if (Directory.Exists(SettingsEngine.DownloadPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = SettingsEngine.DownloadPath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }
    }
}