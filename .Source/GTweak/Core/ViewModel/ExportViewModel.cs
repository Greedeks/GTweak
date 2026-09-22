using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
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
        private bool _isLoading = true;
        private int _totalCount;

        private static readonly Dictionary<(JsonConfigManager.Section Section, string PropertyName), (string Suffix, string EnumName)> _propertyMeta = new Dictionary<(JsonConfigManager.Section, string), (string, string)>();
        private static readonly Dictionary<string, string> _checkboxGroups = new Dictionary<string, string>();

        private static readonly Dictionary<JsonConfigManager.Section, (Type TweaksType, string Suffix)> _sections = new Dictionary<JsonConfigManager.Section, (Type, string)>
        {
            [JsonConfigManager.Section.Confidentiality] = (typeof(ConfidentialityTweaks), "conf"),
            [JsonConfigManager.Section.Interface] = (typeof(InterfaceTweaks), "intf"),
            [JsonConfigManager.Section.Services] = (typeof(ServicesTweaks), "svc"),
            [JsonConfigManager.Section.System] = (typeof(SystemTweaks), "sys"),
            [JsonConfigManager.Section.Packages] = (null, "pkg")
        };

        public ExportSectionModel Confidentiality { get; }
        public ExportSectionModel Interface { get; }
        public ExportSectionModel Packages { get; }
        public ExportSectionModel Services { get; }
        public ExportSectionModel System { get; }

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

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
            foreach (var entry in _sections)
            {
                JsonConfigManager.Section section = entry.Key;
                Type tweaksClass = entry.Value.TweaksType;
                string suffix = entry.Value.Suffix;

                if (tweaksClass == null)
                {
                    continue;
                }

                Type[] nestedTypes = tweaksClass.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);

                foreach (Type enumType in nestedTypes)
                {
                    if (enumType.IsEnum)
                    {
                        FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
                        Array.Sort(fields, (firstField, secondField) => firstField.MetadataToken.CompareTo(secondField.MetadataToken));

                        string currentGroup = string.Empty;

                        foreach (FieldInfo field in fields)
                        {
                            _propertyMeta[(section, field.Name)] = (suffix, enumType.Name);

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

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ExportSectionModel[] sections = { Confidentiality, Interface, Packages, Services, System };
                for (int i = 0; i < sections.Length; i++)
                {
                    InitializeSection(sections[i]);
                }

                UpdateCounters();
                IsLoading = false;

            }, DispatcherPriority.Loaded);
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

            foreach (JProperty property in sec.Properties())
            {
                ParseProperty(section, property);
            }
        }

        private void ParseProperty(ExportSectionModel section, JProperty property)
        {
            if (section.Section == JsonConfigManager.Section.Packages)
            {
                if (property.Name.Equals("EdgeWebView", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                ExportEntryItem entryItem = section.Items.Count > 0 ? section.Items[0] : null;

                if (entryItem == null)
                {
                    ExportPackagesValue groupPackages = new ExportPackagesValue(new ObservableCollection<ExportPackageItem>());
                    entryItem = new ExportEntryItem(nameof(Packages), null, groupPackages);
                    section.Items.Add(entryItem);
                }

                ExportPackagesValue list = (ExportPackagesValue)entryItem.Value;
                bool canRemove = !(property.Value is JValue val) || val.Type != JTokenType.Boolean || (bool)val;
                ExportPackageItem package = new ExportPackageItem(ResolveTitle(property.Name, _sections[section.Section].Suffix), ResolveIcon(property.Name), canRemove);
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

            if (!_propertyMeta.TryGetValue((section.Section, property.Name), out var meta))
            {
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
                    entryItem = new ExportEntryItem(groupKey, ResolveTitle(groupKey, meta.Suffix), new ExportChecklistValue(new ObservableCollection<ExportCheckItem>()))
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
                ExportCheckItem item = new ExportCheckItem(ResolveTitle(property.Name, meta.Suffix), property.Value.ToObject<bool>());
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

            object exportValue = CreateExportValue(meta.EnumName, property.Value);

            if (exportValue != null)
            {
                CreateItem(section, meta.Suffix, property.Name, exportValue);
            }
        }

        private static object CreateExportValue(string enumName, JToken value)
        {
            return enumName switch
            {
                "Toggle" => new ExportToggleValue(value.ToObject<bool>()),
                "Slider" => new ExportValueEntry(value.ToObject<uint>()),
                "Picker" when value.Type == JTokenType.String && TryParseRgb(value.ToString(), out byte r, out byte g, out byte b) => new ExportColorValue(r, g, b),
                "Picker" => new ExportValueEntry(value.ToObject<uint>()),
                _ => null,
            };
        }

        private bool CreateItem(ExportSectionModel section, string suffix, string key, object value)
        {
            ExportEntryItem item = null;
            item = new ExportEntryItem(key, ResolveTitle(key, suffix), value)
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

        private static string ResolveTitle(string name, string suffix)
        {
            if (!string.IsNullOrEmpty(suffix) && Application.Current.TryFindResource($"{name}_{suffix}") is string localized)
            {
                return localized;
            }

            if (Application.Current.TryFindResource(name) is string fallback)
            {
                return fallback;
            }

            return name;
        }

        private static ImageSource ResolveIcon(string name) => Application.Current.TryFindResource($"Img_{name}") as ImageSource;

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