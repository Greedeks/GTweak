using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GTweak.Modules.Common;
using GTweak.Modules.Extensions;
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
            try { await ApplyTweaksWithProgress(_cancellationTokenSource.Token, progress); }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
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
            INIManager iniManager = new INIManager(PathTargets.Files.Config);

            var allSections = new (string Section, Action<string, bool> TweakAction)[]
            {
                (INIManager.SectionConf, _confTweaks.Apply),
                (INIManager.SectionIntf, _intfTweaks.Apply),
                (INIManager.SectionSvc,  _svcTweaks.Apply),
                (INIManager.SectionSys,  null)
            };

            List<(string section, string tweak, string value)> allTweaks = new List<(string section, string tweak, string value)>();

            foreach (var (Section, _) in allSections.Where(s => iniManager.IsThereSection(s.Section)))
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

            string defenderTweak = $"TglButton{(int)SystemToggle.WindowsDefender}";
            var sysTweaks = allTweaks.Where(t => t.section == INIManager.SectionSys).ToList();
            var tweaksToApply = allTweaks.Where(t => t.section != INIManager.SectionSys).Concat(sysTweaks.Where(t => t.tweak != defenderTweak)).Concat(sysTweaks.Where(t => t.tweak == defenderTweak)).ToList();

            foreach (var (section, tweak, value) in tweaksToApply)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    if (section == INIManager.SectionSys)
                    {
                        if (tweak == defenderTweak)
                        {
                            BackgroundQueueManager backgroundQueue = new BackgroundQueueManager();
                            await backgroundQueue.QueueTask(delegate
                            {
                                _sysTweaks.Apply(tweak, Convert.ToBoolean(value), false);
                            });
                            _defenderDisabled = !Convert.ToBoolean(value);
                        }
                        else if (tweak.StartsWith("TglButton"))
                        {
                            _sysTweaks.Apply(tweak, Convert.ToBoolean(value));
                            AddPostAction(tweak.GetPostAction(typeof(SystemToggle)));
                        }
                        else
                        {
                            _sysTweaks.Apply(tweak, Convert.ToUInt32(value));
                        }
                    }
                    else if (section == INIManager.SectionConf)
                    {
                        _confTweaks.Apply(tweak, Convert.ToBoolean(value));
                        AddPostAction(tweak.GetPostAction(typeof(ConfidentialityToggle)));
                    }
                    else if (section == INIManager.SectionIntf)
                    {
                        if (tweak.StartsWith("ColorPicker"))
                        {
                            _intfTweaks.Apply(tweak, value);
                        }
                        else
                        {
                            _intfTweaks.Apply(tweak, Convert.ToBoolean(value));
                            Type enumType = tweak.StartsWith("Checkbox") ? typeof(InterfaceCheckbox) : typeof(InterfaceToggle);
                            AddPostAction(tweak.GetPostAction(enumType));
                        }
                    }
                    else if (section == INIManager.SectionSvc)
                    {
                        _svcTweaks.Apply(tweak, Convert.ToBoolean(value));
                        _pendingAlerts.Add(NotificationManager.AlertType.Restart);
                    }
                }
                catch (Exception ex) { ErrorLogger.LogDebug(ex); }

                appliedTweaks++;
                progress.Report((byte)((double)appliedTweaks / totalTweaks * 100));
                await Task.Delay(700, token);
            }
        }

        private void AddPostAction(PostActionAttribute action)
        {
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