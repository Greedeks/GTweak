using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GTweak.Modules.Common;
using GTweak.Modules.Helpers;
using GTweak.Modules.Managers;
using GTweak.Modules.Tweaks;
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
        private readonly HashSet<NotificationManager.NoticeAction> _notyActions = new HashSet<NotificationManager.NoticeAction>();
        private ExplorerManager.ExplorerAction _expAction = ExplorerManager.ExplorerAction.None;
        private bool _isWDNotyNeed = false;

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
            try { await ApplyTweaksWithProgress(_cancellationTokenSource.Token, progress); } catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }

        private void ReportProgress(byte valueProgress)
        {
            if (valueProgress == 100)
            {
                if (_isWDNotyNeed)
                {
                    NotificationManager.Show("warn", "warn_wd_noty").Perform();
                }
                else
                {
                    switch (_expAction)
                    {
                        case ExplorerManager.ExplorerAction.Restart:
                            ExplorerManager.Restart();
                            break;
                        case ExplorerManager.ExplorerAction.Refresh:
                            ExplorerManager.RefreshDesktop();
                            break;
                        default:
                            break;
                    }

                    if (_notyActions.Count != 0)
                    {
                        NotificationManager.Show().Perform(_notyActions.Max());
                    }
                }

                App.UpdateImport();
                Close();
            }
        }

        private async Task ApplyTweaksWithProgress(CancellationToken token, IProgress<byte> progress)
        {
            INIManager iniManager = new INIManager(PathTargets.Files.Config);

            var allSections = new (string Section, Action<string, bool> TweakAction, Dictionary<Enum, NotificationManager.NoticeAction> NoticeActions, Dictionary<Enum, ExplorerManager.ExplorerAction> ExplorerMapping)[]
            {
                (INIManager.SectionConf, _confTweaks.Apply, NotificationManager.ConfActions, null),
                (INIManager.SectionIntf, _intfTweaks.Apply, NotificationManager.IntfActions, ExplorerManager.IntfActions),
                (INIManager.SectionSvc, _svcTweaks.Apply, null, null),
                (INIManager.SectionSys, null, NotificationManager.SysActions, null)
            };

            List<(string section, string tweak, string value)> allTweaks = new List<(string section, string tweak, string value)>();

            foreach (var (Section, _, _, _) in allSections.Where(s => iniManager.IsThereSection(s.Section)))
            {
                List<string> keys = iniManager.GetKeysOrValue(Section);
                List<string> values = iniManager.GetKeysOrValue(Section, false);
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
                            _sysTweaks.Apply(tweak, Convert.ToBoolean(value));

                            if (NotificationManager.SysActions.TryGetAction(tweak, out NotificationManager.NoticeAction sysAction))
                            {
                                _notyActions.Add(sysAction);
                            }
                        }
                        else if (tweak == "TglButton3")
                        {
                            BackgroundQueueManager backgroundQueue = new BackgroundQueueManager();
                            await backgroundQueue.QueueTask(delegate
                            {
                                _sysTweaks.Apply(tweak, Convert.ToBoolean(value), false);
                            });

                            _isWDNotyNeed = !Convert.ToBoolean(value);
                        }
                        else
                        {
                            _sysTweaks.Apply(tweak, Convert.ToUInt32(value));
                        }
                    }
                    else
                    {
                        var (Section, TweakAction, NoticeActions, ExplorerMapping) = allSections.First(s => s.Section == section);

                        if (section == INIManager.SectionIntf && tweak.StartsWith("ColorPicker"))
                        {
                            _intfTweaks.Apply(tweak, value);
                        }
                        else
                        {
                            TweakAction?.Invoke(tweak, Convert.ToBoolean(value));
                        }

                        if (NoticeActions != null && NoticeActions.TryGetAction(tweak, out NotificationManager.NoticeAction noticeAction))
                        {
                            _notyActions.Add(noticeAction);
                        }

                        if (ExplorerMapping != null && ExplorerMapping.TryGetAction(tweak, out ExplorerManager.ExplorerAction expAction))
                        {
                            if (expAction > _expAction)
                            {
                                _expAction = expAction;
                            }
                        }

                        if (section == INIManager.SectionSvc)
                        {
                            _notyActions.Add(NotificationManager.NoticeAction.Restart);
                        }
                    }
                }
                catch (Exception ex) { ErrorLogger.LogDebug(ex); }

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