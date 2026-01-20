using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class ImportWindow : FluentWindow, IDisposable
    {
        private readonly ConfidentialityTweaks _confTweaks = new ConfidentialityTweaks();
        private readonly InterfaceTweaks _intfTweaks = new InterfaceTweaks();
        private readonly ServicesTweaks _svcTweaks = new ServicesTweaks();
        private readonly SystemTweaks _sysTweaks = new SystemTweaks();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HashSet<NotificationManager.NoticeAction> _requiredActions = new HashSet<NotificationManager.NoticeAction>();
        private bool _isExpRestartNeed = false;

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
            try { await ApplyTweaksWithProgress(_cancellationTokenSource.Token, progress); } catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        private void ReportProgress(byte valueProgress)
        {
            if (valueProgress == 100)
            {
                if (_isExpRestartNeed)
                {
                    ExplorerManager.Restart(new Process());
                }

                if (_requiredActions.Count != 0)
                {
                    NotificationManager.Show().Perform(_requiredActions.Max());
                }

                App.UpdateImport();
                Close();
            }
        }

        private async Task ApplyTweaksWithProgress(CancellationToken token, IProgress<byte> progress)
        {
            INIManager iniManager = new INIManager(PathLocator.Files.Config);

            var allSections = new (string Section, Action<string, bool> TweakAction, IEnumerable<KeyValuePair<string, NotificationManager.NoticeAction>> NoticeActions, IEnumerable<KeyValuePair<string, bool>> ExplorerMapping)[]
            {
                (INIManager.SectionConf, _confTweaks.ApplyTweaks, NotificationManager.ConfActions, null),
                (INIManager.SectionIntf, _intfTweaks.ApplyTweaks, NotificationManager.IntfActions, ExplorerManager.IntfMapping),
                (INIManager.SectionSvc, _svcTweaks.ApplyTweaks, null, null),
                (INIManager.SectionSys, null, NotificationManager.SysActions, null)
            };

            var allTweaks = new List<(string section, string tweak, string value)>();

            foreach (var (Section, _, _, _) in allSections.Where(s => iniManager.IsThereSection(s.Section)))
            {
                var keys = iniManager.GetKeysOrValue(Section);
                var values = iniManager.GetKeysOrValue(Section, false);
                allTweaks.AddRange(keys.Zip(values, (t, v) => (Section, t, v)));
            }

            int totalTweaks = allTweaks.Count;
            int appliedTweaks = 0;

            if (totalTweaks == 0)
            {
                progress.Report(100);
                return;
            }

            var sysTweaks = allTweaks.Where(t => t.section == INIManager.SectionSys).ToList();
            var sysTweaksLast = sysTweaks.Where(t => t.tweak == "TglButton3").ToList();
            var sysTweaksFirst = sysTweaks.Where(t => t.tweak != "TglButton3").ToList();

            var tweaksToApply = allTweaks.Where(t => t.section != INIManager.SectionSys).Concat(sysTweaksFirst).Concat(sysTweaksLast).ToList();

            foreach (var (section, tweak, value) in tweaksToApply)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    if (section == INIManager.SectionSys)
                    {
                        if (tweak.StartsWith("TglButton") && tweak != "TglButton3")
                        {
                            _sysTweaks.ApplyTweaks(tweak, Convert.ToBoolean(value));
                        }
                        else if (tweak == "TglButton3")
                        {
                            BackgroundQueue backgroundQueue = new BackgroundQueue();
                            await backgroundQueue.QueueTask(delegate
                            {
                                _sysTweaks.ApplyTweaks(tweak, Convert.ToBoolean(value), false);
                            });

                            if (!Convert.ToBoolean(value))
                            {
                                CommandExecutor.RunCommand($"/c timeout /t 10 && del /f \"{PathLocator.Executable.NSudo}\"");
                            }
                        }
                        else
                        {
                            _sysTweaks.ApplyTweaksSlider(tweak, Convert.ToUInt32(value));
                        }

                        foreach (var act in NotificationManager.SysActions.Where(a => a.Key == tweak))
                        {
                            _requiredActions.Add(act.Value);
                        }
                    }
                    else
                    {
                        var (Section, TweakAction, NoticeActions, ExplorerMapping) =
                            allSections.First(s => s.Section == section);

                        TweakAction?.Invoke(tweak, Convert.ToBoolean(value));

                        if (NoticeActions != null)
                        {
                            foreach (var act in NoticeActions.Where(a => a.Key == tweak))
                            {
                                _requiredActions.Add(act.Value);
                            }
                        }

                        if (ExplorerMapping != null && ExplorerMapping.Any(a => a.Key == tweak && a.Value))
                        {
                            _isExpRestartNeed = true;
                        }

                        if (section == INIManager.SectionSvc)
                        {
                            _requiredActions.Add(NotificationManager.NoticeAction.Restart);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogging.LogDebug(ex);
                }

                appliedTweaks++;
                progress.Report((byte)((double)appliedTweaks / totalTweaks * 100));
                await Task.Delay(700, token);
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
