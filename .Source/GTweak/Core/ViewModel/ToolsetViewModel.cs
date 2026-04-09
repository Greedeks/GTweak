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
using GTweak.Core.Services;
using GTweak.Utilities.Controls;
using Ookii.Dialogs.Wpf;

namespace GTweak.Core.ViewModel
{
    internal class ToolsetViewModel : ViewModelBase
    {
        public ObservableCollection<ToolsetItem> Tools { get; } = new ObservableCollection<ToolsetItem>();
        private readonly FuzzySearchService _fuzzyService = new FuzzySearchService();

        private string _searchText = string.Empty;

        public ICollectionView ToolsView { get; }
        public ICommand SelectFolderCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand ClearCommand { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                value ??= string.Empty;

                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    ToolsView?.Refresh();
                }
            }
        }

        public ToolsetViewModel()
        {
            SelectFolderCommand = new RelayCommand(_ => SelectFolder());
            OpenFolderCommand = new RelayCommand(_ => OpenFolder());
            ClearCommand = new RelayCommand(_ => SearchText = string.Empty);

            ToolsView = CollectionViewSource.GetDefaultView(Tools);
            ToolsView.Filter = FilterTools;

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

                    foreach (XElement appElement in doc.Descendants("App"))
                    {
                        ToolsetModel model = new ToolsetModel(appElement);
                        Tools.Add(new ToolsetItem(model));
                    }
                }

                ToolsView?.Refresh();
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        private bool FilterTools(object obj)
        {
            if (obj is ToolsetItem item)
            {
                if (string.IsNullOrWhiteSpace(_searchText))
                {
                    return true;
                }

                return _fuzzyService.IsMatch(item.AppName, _searchText) || _fuzzyService.IsMatch(item.AuthorName, _searchText);
            }

            return false;
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