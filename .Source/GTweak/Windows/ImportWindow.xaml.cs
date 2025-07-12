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
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HashSet<string> requiredActions = new HashSet<string>();
        private bool isExpRestartNeed = false;

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
            BeginAnimation(OpacityProperty, FadeAnimation.FadeIn(1, 0.2));
            Progress<byte> progress = new Progress<byte>(ReportProgress);
            try { await ApplyTweaksWithProgress(_cancellationTokenSource.Token, progress); } catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        private void ReportProgress(byte valueProgress)
        {
            if (valueProgress == 100)
            {
                if (isExpRestartNeed)
                    ExplorerManager.Restart(new Process());

                if (requiredActions.Contains("restart"))
                    new NotificationManager().Show("restart");
                else if (requiredActions.Contains("logout"))
                    new NotificationManager().Show("logout");

                App.UpdateImport();
                BeginAnimation(OpacityProperty, FadeAnimation.FadeTo(0.1, () => { Close(); }));
            }
        }

        private async Task ApplyTweaksWithProgress(CancellationToken token, IProgress<byte> progress)
        {
            INIManager iniManager = new INIManager(PathLocator.Files.Config);

            var allSections = new (string Section, Action<string, bool> TweakAction, IEnumerable<KeyValuePair<string, string>> NotificationActions, IEnumerable<KeyValuePair<string, bool>> ExplorerMapping)[]
            {
               (INIManager.SectionConf, ConfidentialityTweaks.ApplyTweaks, NotificationManager.ConfActions, null),
               (INIManager.SectionIntf, InterfaceTweaks.ApplyTweaks, NotificationManager.IntfActions, ExplorerManager.IntfMapping),
               (INIManager.SectionSvc, ServicesTweaks.ApplyTweaks, null, null),
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
                            SystemTweaks.ApplyTweaks(tweak, Convert.ToBoolean(value));
                        else if (tweak == "TglButton8")
                        {
                            BackgroundQueue backgroundQueue = new BackgroundQueue();
                            await backgroundQueue.QueueTask(delegate { SystemTweaks.ApplyTweaks(tweak, Convert.ToBoolean(value), false); });
                        }
                        else
                            SystemTweaks.ApplyTweaksSlider(tweak, Convert.ToUInt32(value));

                        foreach (var act in NotificationManager.SysActions.Where(get => get.Key == tweak))
                            requiredActions.Add(act.Value);
                    }
                    else
                    {
                        var (Section, TweakAction, NotificationActions, ExplorerMapping) = allSections.First(s => s.Section == section);

                        TweakAction?.Invoke(tweak, Convert.ToBoolean(value));

                        if (NotificationActions != null)
                        {
                            foreach (var act in NotificationActions.Where(get => get.Key == tweak))
                                requiredActions.Add(act.Value);
                        }

                        if (ExplorerMapping != null && ExplorerMapping.Any(get => get.Key == tweak && get.Value == true))
                            isExpRestartNeed = true;

                        if (section == INIManager.SectionSvc)
                            requiredActions.Add("restart");
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

