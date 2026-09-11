using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GTweak.Animations;
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
        internal static readonly DependencyProperty CurrentSectionProperty =
            DependencyProperty.Register(nameof(CurrentSection), typeof(JsonConfigManager.Section), typeof(ImportWindow), new PropertyMetadata(JsonConfigManager.Section.Confidentiality, OnCurrentSectionChanged));

        internal JsonConfigManager.Section CurrentSection
        {
            get => (JsonConfigManager.Section)GetValue(CurrentSectionProperty);
            set => SetValue(CurrentSectionProperty, value);
        }

        private readonly ConfidentialityTweaks _confTweaks = new ConfidentialityTweaks();
        private readonly InterfaceTweaks _intfTweaks = new InterfaceTweaks();
        private readonly ServicesTweaks _svcTweaks = new ServicesTweaks();
        private readonly SystemTweaks _sysTweaks = new SystemTweaks();

        private readonly BackgroundQueueManager _backgroundQueue = new BackgroundQueueManager();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HashSet<NotificationManager.AlertType> _pendingAlerts = new HashSet<NotificationManager.AlertType>();
        private readonly object _shellLock = new object();
        private ExplorerManager.ShellType _shellType = ExplorerManager.ShellType.None;
        private bool _defenderDisabled = false, _isCompleted = false;

        private Brush BrushDone => (Brush)FindResource("Brush_Background_Labels");
        private Brush BrushActive => (Brush)FindResource("Brush_Accent");
        private Brush BrushInactive => (Brush)FindResource("Brush_Separator");


        public ImportWindow(in string importedFile)
        {
            InitializeComponent();
            ImportedFile.Text = importedFile;
        }

        private static void OnCurrentSectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImportWindow window && e.NewValue is JsonConfigManager.Section section)
            {
                window.ActivateSegment((int)section);
            }
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            Progress<byte> progress = new Progress<byte>(ReportProgress);
            try { await ApplyTweaksWithProgress(_cancellationTokenSource.Token, progress); }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e?.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private async void ReportProgress(byte valueProgress)
        {
            AnimateProgressBar(valueProgress);
            NumberAnimation.SetValue(PercentLabel, valueProgress);

            if (valueProgress >= 100 && !_isCompleted)
            {
                _isCompleted = true;
                await ShowDoneState();
            }
        }

        private void ReportSection(JsonConfigManager.Section section) => Dispatcher.Invoke(() => CurrentSection = section);

        private async Task ApplyTweaksWithProgress(CancellationToken token, IProgress<byte> progress)
        {
            JObject configRoot = JsonConfigManager.Load(string.IsNullOrWhiteSpace(PathTargets.Files.Config) ? PathTargets.Files.BackupConfig : PathTargets.Files.Config);

            if (configRoot == null)
            {
                progress.Report(100);
                return;
            }

            bool shouldRemoveWebView = false;
            if (configRoot[JsonConfigManager.Section.Packages.ToString()] is JObject packages && packages["EdgeWebView"] is JValue jvalue && jvalue.Type == JTokenType.Boolean)
            {
                shouldRemoveWebView = (bool)jvalue;
            }

            var tweaksToApply = new List<(JsonConfigManager.Section Section, string Tweak, string Value)>();
            (JsonConfigManager.Section Section, string Tweak, string Value)? defenderEntry = null;

            foreach (JsonConfigManager.Section section in (JsonConfigManager.Section[])Enum.GetValues(typeof(JsonConfigManager.Section)))
            {
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
            if (totalTweaks == 0)
            {
                progress.Report(100);
                return;
            }

            int appliedTweaks = 0;

            void ReportStep()
            {
                int current = Interlocked.Increment(ref appliedTweaks);
                progress.Report((byte)Math.Min(98, (int)((double)current / totalTweaks * 100)));
            }

            for (int i = 0; i < tweaksToApply.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                var (section, tweak, value) = tweaksToApply[i];

                if (i == 0 || tweaksToApply[i - 1].Section != section)
                {
                    ReportSection(section);
                }

                try
                {
                    if (section == JsonConfigManager.Section.Confidentiality)
                    {
                        _confTweaks.Apply(tweak, Convert.ToBoolean(value));
                        AddPostAction(tweak.GetPostAction(typeof(ConfidentialityTweaks.Toggle)));

                        ReportStep();
                        await Task.Delay(200, token);
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

                        ReportStep();
                        await Task.Delay(200, token);
                    }
                    else if (section == JsonConfigManager.Section.Packages)
                    {
                        bool packageState = Convert.ToBoolean(value);

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

                        if (PackageStorage.PackagesDetails.TryGetValue(tweak, out var details))
                        {
                            if (details.ShellType > _shellType)
                            {
                                _shellType = details.ShellType;
                            }
                        }

                        ReportStep();
                    }
                    else if (section == JsonConfigManager.Section.Services)
                    {
                        _svcTweaks.Apply(tweak, Convert.ToBoolean(value));
                        _pendingAlerts.Add(NotificationManager.AlertType.Restart);

                        ReportStep();
                        await Task.Delay(200, token);
                    }
                    else if (section == JsonConfigManager.Section.System)
                    {
                        if (tweak == nameof(SystemTweaks.Toggle.WindowsDefender))
                        {
                            _defenderDisabled = !Convert.ToBoolean(value);
                            await Task.Run(() => _sysTweaks.Apply(tweak, Convert.ToBoolean(value), false), token);
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

                        ReportStep();
                        await Task.Delay(200, token);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogDebug(ex);
                    ReportStep();
                }
            }

            if (tweaksToApply.Any(t => t.Section == JsonConfigManager.Section.Packages))
            {
                await Task.Run(() => new AppxPackageHandler().GetInstalledPackages());
            }

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

        private void ActivateSegment(int activeIndex)
        {
            int index = 0;
            foreach (Border seg in SegmentsGrid.Children.OfType<Border>())
            {
                Brush targetBrush = index < activeIndex ? BrushDone : (index == activeIndex ? BrushActive : BrushInactive);

                if (!Equals(seg.Background, targetBrush))
                {
                    seg.BeginAnimation(Border.BackgroundProperty, new BrushAnimation
                    {
                        From = seg.Background,
                        To = targetBrush,
                        Duration = TimeSpan.FromMilliseconds(250)
                    });
                }

                index++;
            }
        }

        private void AnimateProgressBar(double targetPercent)
        {
            double trackWidth = ProgressTrack.ActualWidth > 0 ? ProgressTrack.ActualWidth : 340;
            ProgressFill.BeginAnimation(WidthProperty, AnimationFactory.CreateIn(ProgressFill.ActualWidth, (double)Math.Min(trackWidth, trackWidth * (targetPercent / 100.0)), 0.45, useCubicEase: true));
        }

        private async Task ShowDoneState()
        {
            ActivateSegment(int.MaxValue);

            await Task.Delay(550);

            TaskCompletionSource<bool> fadeOutTcs = new TaskCompletionSource<bool>();
            PanelProgress.BeginAnimation(UIElement.OpacityProperty, AnimationFactory.CreateIn(1.0, 0.0, 0.25, () => fadeOutTcs.TrySetResult(true)));
            await fadeOutTcs.Task;

            PanelProgress.Visibility = Visibility.Collapsed;
            PanelDone.Opacity = 0.0;
            PanelDone.Visibility = Visibility.Visible;

            TaskCompletionSource<bool> fadeInTcs = new TaskCompletionSource<bool>();
            PanelDone.BeginAnimation(UIElement.OpacityProperty, AnimationFactory.CreateIn(0.0, 1.0, 0.3, () => fadeInTcs.TrySetResult(true)));
            await fadeInTcs.Task;

            await Task.Delay(2000);

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