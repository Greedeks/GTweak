using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GTweak.Modules.Common;
using GTweak.Modules.Extensions;
using GTweak.Modules.Managers;
using GTweak.Modules.Storage;
using GTweak.Modules.Tweaks;
using Newtonsoft.Json.Linq;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class ImportWindow : FluentWindow, IDisposable
    {
        private readonly ConfidentialityTweaks _confTweaks = new ConfidentialityTweaks();
        private readonly InterfaceTweaks _intfTweaks = new InterfaceTweaks();
        private readonly ServicesTweaks _svcTweaks = new ServicesTweaks();
        private readonly SystemTweaks _sysTweaks = new SystemTweaks();

        private readonly BackgroundQueueManager _backgroundQueue = new BackgroundQueueManager();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HashSet<NotificationManager.AlertType> _pendingAlerts = new HashSet<NotificationManager.AlertType>();
        private ExplorerManager.ShellType _shellType = ExplorerManager.ShellType.None;
        private bool _defenderDisabled = false;

        public ImportWindow(in string importedFile)
        {
            InitializeComponent();
            ImportedFile.Text = importedFile;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e?.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            Progress<byte> progress = new Progress<byte>(ReportProgress);
            try
            {
                await ApplyTweaksWithProgress(_cancellationTokenSource.Token, progress);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogDebug(ex);
            }
        }

        private void ReportProgress(byte valueProgress)
        {
            if (valueProgress == 100)
            {
                if (_defenderDisabled)
                {
                    NotificationManager.Warn("warn_wd_noty").Perform();
                }
                else
                {
                    ExplorerManager.Handle(_shellType);

                    if (_pendingAlerts.Count != 0)
                    {
                        NotificationManager.Default().Perform(_pendingAlerts.Max());
                    }
                }

                App.UpdateImport();
                Close();
            }
        }

        private async Task ApplyTweaksWithProgress(CancellationToken token, IProgress<byte> progress)
        {
            JObject configRoot = JsonConfigManager.Load(string.IsNullOrWhiteSpace(PathTargets.Files.Config) ? PathTargets.Files.BackupConfig : PathTargets.Files.Config);

            if (configRoot == null)
            {
                progress.Report(100);
                return;
            }

            bool shouldRemoveWebView = false;
            if (configRoot[JsonConfigManager.Section.Packages.ToString()] is JObject packages)
            {
                if (packages["EdgeWebView"] is JValue jvalue && jvalue.Type == JTokenType.Boolean)
                {
                    shouldRemoveWebView = (bool)jvalue;
                }
            }

            var tweaksToApply = new List<(JsonConfigManager.Section Section, string Tweak, string Value)>();
            (JsonConfigManager.Section Section, string Tweak, string Value)? defenderEntry = null;

            JsonConfigManager.Section[] sections = (JsonConfigManager.Section[])Enum.GetValues(typeof(JsonConfigManager.Section));
            for (int i = 0; i < sections.Length; i++)
            {
                JsonConfigManager.Section section = sections[i];
                if (configRoot[section.ToString()] is JObject sectionObj && sectionObj.HasValues)
                {
                    foreach (JProperty prop in sectionObj.Properties())
                    {
                        if (section == JsonConfigManager.Section.Packages && prop.Name == "EdgeWebView")
                        {
                            continue;
                        }

                        if (section == JsonConfigManager.Section.System && prop.Name == nameof(SystemTweaks.Toggle.WindowsDefender))
                        {
                            defenderEntry = (section, prop.Name, prop.Value.ToString());
                        }
                        else
                        {
                            tweaksToApply.Add((section, prop.Name, prop.Value.ToString()));
                        }
                    }
                }
            }

            if (defenderEntry.HasValue)
            {
                tweaksToApply.Add(defenderEntry.Value);
            }

            int totalTweaks = tweaksToApply.Count;
            int appliedTweaks = 0;

            if (totalTweaks == 0)
            {
                progress.Report(100);
                return;
            }

            for (int i = 0; i < tweaksToApply.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                (JsonConfigManager.Section section, string tweak, string value) = tweaksToApply[i];

                try
                {
                    if (section == JsonConfigManager.Section.Confidentiality)
                    {
                        _confTweaks.Apply(tweak, Convert.ToBoolean(value));
                        AddPostAction(tweak.GetPostAction(typeof(ConfidentialityTweaks.Toggle)));
                    }
                    else if (section == JsonConfigManager.Section.Interface)
                    {
                        if (Enum.TryParse<InterfaceTweaks.Color>(tweak, out _))
                        {
                            _intfTweaks.Apply(tweak, value);
                        }
                        else if (Enum.TryParse<InterfaceTweaks.Checkbox>(tweak, out _))
                        {
                            _intfTweaks.Apply(tweak, Convert.ToBoolean(value));
                            AddPostAction(tweak.GetPostAction(typeof(InterfaceTweaks.Checkbox)));
                        }
                        else
                        {
                            _intfTweaks.Apply(tweak, Convert.ToBoolean(value));
                            AddPostAction(tweak.GetPostAction(typeof(InterfaceTweaks.Toggle)));
                        }
                    }
                    else if (section == JsonConfigManager.Section.Services)
                    {
                        _svcTweaks.Apply(tweak, Convert.ToBoolean(value));
                        _pendingAlerts.Add(NotificationManager.AlertType.Restart);
                    }
                    else if (section == JsonConfigManager.Section.System)
                    {
                        if (tweak == nameof(SystemTweaks.Toggle.WindowsDefender))
                        {
                            await _backgroundQueue.QueueTask(delegate
                            {
                                _sysTweaks.Apply(tweak, Convert.ToBoolean(value), false);
                            });
                            _defenderDisabled = !Convert.ToBoolean(value);
                        }
                        else if (Enum.TryParse<SystemTweaks.Slider>(tweak, out _))
                        {
                            _sysTweaks.Apply(tweak, Convert.ToUInt32(value));
                        }
                        else
                        {
                            _sysTweaks.Apply(tweak, Convert.ToBoolean(value));
                            AddPostAction(tweak.GetPostAction(typeof(SystemTweaks.Toggle)));
                        }
                    }
                    else if (section == JsonConfigManager.Section.Packages)
                    {
                        bool packageState = Convert.ToBoolean(value);

                        await _backgroundQueue.QueueTask(async () =>
                        {
                            if (tweak == "OneDrive")
                            {
                                if (packageState)
                                {
                                    await AppxPackageHandler.RemoveAppxPackage("OneDrive");
                                }
                                else
                                {
                                    await AppxPackageHandler.RestoreOneDriveFolder();
                                }
                            }
                            else if (tweak == "Edge")
                            {
                                if (packageState)
                                {
                                    await AppxPackageHandler.RemoveAppxPackage("Edge", shouldRemoveWebView);
                                }
                            }
                            else if (packageState)
                            {
                                await AppxPackageHandler.RemoveAppxPackage(tweak);
                            }
                        });

                        if (PackageStorage.PackagesDetails.TryGetValue(tweak, out var details) && details.ShellType > _shellType)
                        {
                            _shellType = details.ShellType;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogDebug(ex);
                }

                appliedTweaks++;
                progress.Report((byte)((double)appliedTweaks / totalTweaks * 100));
                await Task.Delay(700, token);
            }

            await _backgroundQueue.WaitForCompletion();

            progress.Report(100);
        }

        private void AddPostAction(PostActionAttribute action)
        {
            if (action == null)
            {
                return;
            }

            if (action.HasAlert())
            {
                _pendingAlerts.Add(action.Alert);
            }

            if (action.Shell > _shellType)
            {
                _shellType = action.Shell;
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        protected override void OnClosed(EventArgs e)
        {
            Dispose();
            base.OnClosed(e);
        }
    }
}