using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GTweak.Core.Base;
using GTweak.Core.DataContracts;
using GTweak.Core.Items;
using GTweak.Core.Model;
using GTweak.Modules.Common;
using GTweak.Modules.Managers;
using GTweak.Modules.Tweaks;
using Newtonsoft.Json.Linq;
using Ookii.Dialogs.Wpf;

namespace GTweak.Core.ViewModel
{
    internal sealed class ExportViewModel : PropertyChangedBase
    {
        private readonly JObject _сonfig = JsonConfigManager.Load(PathTargets.Files.BackupConfig) ?? new JObject();
        private int _totalCount;

        private static readonly Dictionary<string, string> _checkboxGroups = new Dictionary<string, string>();
        private static readonly Dictionary<JsonConfigManager.Section, string> _suffixes = new Dictionary<JsonConfigManager.Section, string>
        {
            [JsonConfigManager.Section.Confidentiality] = "conf",
            [JsonConfigManager.Section.Interface] = "intf",
            [JsonConfigManager.Section.Services] = "serv",
            [JsonConfigManager.Section.System] = "sys"
        };

        public ExportSectionModel Confidentiality { get; }
        public ExportSectionModel Interface { get; }
        public ExportSectionModel Packages { get; }
        public ExportSectionModel Services { get; }
        public ExportSectionModel System { get; }

        public int TotalCount
        {
            get => _totalCount;
            private set
            {
                if (_totalCount != value)
                {
                    _totalCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }

        static ExportViewModel()
        {
            string currentGroup = string.Empty;

            FieldInfo[] fields = typeof(InterfaceTweaks.Checkbox).GetFields(BindingFlags.Public | BindingFlags.Static);
            Array.Sort(fields, (a, b) => a.MetadataToken.CompareTo(b.MetadataToken));

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                GroupAttribute groupAttr = field.GetCustomAttribute<GroupAttribute>();

                if (groupAttr != null)
                {
                    currentGroup = groupAttr.Key;
                }

                if (!string.IsNullOrEmpty(currentGroup))
                {
                    _checkboxGroups[field.Name] = currentGroup;
                }
            }
        }

        public ExportViewModel()
        {
            Confidentiality = new ExportSectionModel(JsonConfigManager.Section.Confidentiality);
            Interface = new ExportSectionModel(JsonConfigManager.Section.Interface);
            Packages = new ExportSectionModel(JsonConfigManager.Section.Packages);
            Services = new ExportSectionModel(JsonConfigManager.Section.Services);
            System = new ExportSectionModel(JsonConfigManager.Section.System);

            SaveCommand = new RelayCommand(param => SaveConfig(param as Window), _ => TotalCount > 0);

            foreach (ExportSectionModel section in new[] { Confidentiality, Interface, Packages, Services, System })
            {
                InitializeSection(section);
            }

            UpdateCounters();
        }

        private void SaveConfig(Window window)
        {
            VistaSaveFileDialog vistaSaveFileDialog = new VistaSaveFileDialog
            {
                FileName = "Config GTweak",
                Filter = "(*.JSON)|*.JSON",
                DefaultExt = "json",
                AddExtension = true,
                RestoreDirectory = true
            };

            if (vistaSaveFileDialog.ShowDialog() == true)
            {
                JsonConfigManager.Save(_сonfig, vistaSaveFileDialog.FileName);
                NotificationManager.Info("export_success_noty").Perform();
                window?.Close();
            }
        }

        private void InitializeSection(ExportSectionModel section)
        {
            if (!(_сonfig[section.Section.ToString()] is JObject sec) || !sec.HasValues)
            {
                return;
            }

            string suffix = _suffixes.TryGetValue(section.Section, out var s) ? s : string.Empty;

            foreach (JProperty property in sec.Properties())
            {
                ParseProperty(section, property, suffix);
            }
        }

        private void ParseProperty(ExportSectionModel section, JProperty property, string suffix)
        {
            if (section.Section == JsonConfigManager.Section.Packages)
            {
                ExportEntryItem entryItem = section.Items.Count > 0 ? section.Items[0] : null;

                if (entryItem == null)
                {
                    ExportPackagesValue groupPackages = new ExportPackagesValue(new ObservableCollection<ExportPackageItem>());
                    entryItem = new ExportEntryItem(nameof(Packages), null, groupPackages);
                    section.Items.Add(entryItem);
                }

                ExportPackagesValue list = (ExportPackagesValue)entryItem.Value;
                bool canRemove = !(property.Value is JValue val) || val.Type != JTokenType.Boolean || (bool)val;
                ImageSource icon = Application.Current.TryFindResource($"Img_{property.Name}") as ImageSource;

                ExportPackageItem package = new ExportPackageItem(property.Name, icon, canRemove);
                package.RemoveCommand = new RelayCommand(_ =>
                {
                    if (_сonfig?[section.Section.ToString()] is JObject sec)
                    {
                        sec.Remove(property.Name);
                    }

                    list.Packages.Remove(package);

                    if (list.Packages.Count == 0)
                    {
                        section.Items.Remove(entryItem);
                        _сonfig.Remove(section.Section.ToString());
                        section.RefreshState();
                    }

                    UpdateCounters();
                });

                list.Packages.Add(package);
                return;
            }

            if (_checkboxGroups.TryGetValue(property.Name, out string groupKey))
            {
                ExportEntryItem entryItem = null;

                for (int i = 0; i < section.Items.Count; i++)
                {
                    if (section.Items[i].Key == groupKey)
                    {
                        entryItem = section.Items[i];
                        break;
                    }
                }

                if (entryItem == null)
                {
                    entryItem = new ExportEntryItem(groupKey, ResolveTitle("exblock", groupKey, suffix), new ExportChecklistValue(new ObservableCollection<ExportCheckItem>()))
                    {
                        RemoveCommand = new RelayCommand(_ =>
                        {
                            section.Items.Remove(entryItem);

                            if (_сonfig?[section.Section.ToString()] is JObject sec)
                            {
                                foreach (var pair in _checkboxGroups)
                                {
                                    if (pair.Value == groupKey)
                                    {
                                        sec.Remove(pair.Key);
                                    }
                                }
                            }

                            section.RefreshState();
                            UpdateCounters();
                        })
                    };

                    section.Items.Add(entryItem);
                }

                ExportChecklistValue checklist = (ExportChecklistValue)entryItem.Value;
                string label = ResolveTitle("chk", property.Name, suffix);
                bool state = property.Value.ToObject<bool>();

                var item = new ExportCheckItem(label, state);
                item.RemoveCommand = new RelayCommand(_ =>
                {
                    if (_сonfig?[section.Section.ToString()] is JObject sec)
                    {
                        sec.Remove(property.Name);
                    }

                    checklist.Items.Remove(item);

                    if (checklist.Items.Count == 0)
                    {
                        section.Items.Remove(entryItem);
                        section.RefreshState();
                    }

                    UpdateCounters();
                });

                checklist.Items.Add(item);
                return;
            }

            if (property.Value.Type == JTokenType.String && TryParseRgb(property.Value.ToString(), out byte r, out byte g, out byte b))
            {
                CreateItem(section, "color", suffix, property.Name, new ExportColorValue(r, g, b));
                return;
            }

            if (property.Value.Type == JTokenType.Integer)
            {
                CreateItem(section, "slider", suffix, property.Name, new ExportSliderValue(property.Value.ToObject<uint>()));
                return;
            }

            if (property.Value.Type == JTokenType.Boolean)
            {
                CreateItem(section, "tgl", suffix, property.Name, new ExportToggleValue(property.Value.ToObject<bool>()));
                return;
            }
        }

        private bool CreateItem(ExportSectionModel section, string prefix, string suffix, string key, object value)
        {
            ExportEntryItem item = null;
            item = new ExportEntryItem(key, ResolveTitle(prefix, key, suffix), value)
            {
                RemoveCommand = new RelayCommand(_ =>
                {
                    section.Items.Remove(item);
                    JsonConfigManager.Remove(section.Section, key, _сonfig);
                    section.RefreshState();
                    UpdateCounters();
                })
            };

            section.Items.Add(item);
            return true;
        }

        private void UpdateCounters()
        {
            Confidentiality.RefreshState();
            Interface.RefreshState();
            Packages.RefreshState();
            Services.RefreshState();
            System.RefreshState();

            TotalCount = CalculateTotalCount();
        }

        private int CalculateTotalCount() => Confidentiality.Count + Interface.Count + Packages.Count + Services.Count + System.Count;

        private static string ResolveTitle(string prefix, string name, string suffix)
        {
            if (!string.IsNullOrEmpty(suffix))
            {
                string keyWithSuffix = $"{prefix}_{name}_{suffix}";
                if (Application.Current.TryFindResource(keyWithSuffix) is string localized)
                {
                    return localized;
                }
            }

            string key = $"{prefix}_{name}";
            if (Application.Current.TryFindResource(key) is string fallback)
            {
                return fallback;
            }

            return name;
        }

        private static bool TryParseRgb(string raw, out byte r, out byte g, out byte b)
        {
            r = g = b = 0;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            string[] parts = raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 3 && byte.TryParse(parts[0], out r) && byte.TryParse(parts[1], out g) && byte.TryParse(parts[2], out b);
        }
    }
}