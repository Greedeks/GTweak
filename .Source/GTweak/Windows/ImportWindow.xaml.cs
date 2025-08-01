using GTweak.Utilities.Animation;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GTweak.Windows
{
    public partial class ImportWindow : Window
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
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.2));
            Progress<byte> progress = new Progress<byte>(ReportProgress);
            try { await ApplyTweaksWithProgress(_cancellationTokenSource.Token, progress); } catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        private void ReportProgress(byte valueProgress)
        {
            if (valueProgress == 100)
            {
                if (_isExpRestartNeed)
                    ExplorerManager.Restart(new Process());

                if (_requiredActions.Count != 0)
                    new NotificationManager().Show().Perform(_requiredActions.Max());

                App.UpdateImport();
                BeginAnimation(OpacityProperty, FactoryAnimation.CreateTo(0.1, () => { Close(); }));
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

            foreach (var (Section, _, _, _) in allSections.Where(sectionInfo => iniManager.IsThereSection(sectionInfo.Section)))
            {
                var keys = iniManager.GetKeysOrValue(Section);
                var values = iniManager.GetKeysOrValue(Section, false);
                allTweaks.AddRange(keys.Zip(values, (t, v) => (Section, t, v)));
            }

            int totalTweaks = allTweaks.Count;
            int appliedTweaks = 0;

            if (totalTweaks == 0) progress.Report(100);

            foreach (var (section, tweak, value) in allTweaks)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    if (section == INIManager.SectionSys)
                    {
                        if (tweak.StartsWith("TglButton") && tweak != "TglButton8")
                            _sysTweaks.ApplyTweaks(tweak, Convert.ToBoolean(value));
                        else if (tweak == "TglButton8")
                        {
                            BackgroundQueue backgroundQueue = new BackgroundQueue();
                            await backgroundQueue.QueueTask(delegate { _sysTweaks.ApplyTweaks(tweak, Convert.ToBoolean(value), false); });
                        }
                        else
                            _sysTweaks.ApplyTweaksSlider(tweak, Convert.ToUInt32(value));

                        foreach (var act in NotificationManager.SysActions.Where(get => get.Key == tweak))
                            _requiredActions.Add(act.Value);
                    }
                    else
                    {
                        var (Section, TweakAction, NoticeActions, ExplorerMapping) = allSections.First(s => s.Section == section);

                        TweakAction?.Invoke(tweak, Convert.ToBoolean(value));

                        if (NoticeActions != null)
                        {
                            foreach (var act in NoticeActions.Where(get => get.Key == tweak))
                                _requiredActions.Add(act.Value);
                        }

                        if (ExplorerMapping != null && ExplorerMapping.Any(get => get.Key == tweak && get.Value == true))
                            _isExpRestartNeed = true;

                        if (section == INIManager.SectionSvc)
                            _requiredActions.Add(NotificationManager.NoticeAction.Restart);
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }

                appliedTweaks++;
                progress.Report((byte)((double)appliedTweaks / totalTweaks * 100));
                await Task.Delay(700, token);
            }
        }

    }
}

