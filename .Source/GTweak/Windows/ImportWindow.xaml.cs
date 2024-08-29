using GTweak.Utilities;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GTweak.Windows
{

    public partial class ImportWindow : Window
    {
        private bool isRestartNeed = false, isLogoutNeed = false;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public ImportWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Progress<int> _progress = new Progress<int>(ReportProgress);
            try { await SortByPageDate(_cancellationTokenSource.Token, _progress); } catch { }
        }

        private void ReportProgress(int _valueProgress)
        {
            if (_valueProgress == 100)
            {
                if (isRestartNeed)
                    new ViewNotification().Show("restart");
                else if (!isRestartNeed && isLogoutNeed)
                    new ViewNotification().Show("logout");
                App.UpdateImport();
                Close();
            }
        }

        private async Task SortByPageDate(CancellationToken _token, IProgress<int> _progress)
        {
            List<string> tempSection = new List<string>(), tempKeys = new List<string>();
   
            INIManager iniManager = new INIManager(Settings.PathConfig);

            for (int i = 1; i <= 100; i++)
            {
                _token.ThrowIfCancellationRequested();

                if (i == 2 & iniManager.IsThereSection("Confidentiality Tweaks"))
                {
                    tempSection.Clear(); tempKeys.Clear();

                    tempSection = iniManager.GetSection("Confidentiality Tweaks");
                    tempKeys = iniManager.GetKeys("Confidentiality Tweaks");

                    var acceptanceList = tempSection.Zip(tempKeys, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700);
                        Parallel.Invoke(() => ConfidentialityTweaks.UseСonfidentiality(set.Tweak, Convert.ToBoolean(set.Value)));

                        switch (set.Tweak)
                        {
                            case "TglButton8":
                            case "TglButton15":
                                isRestartNeed = true;
                                break;
                        }
                    }
                }

                if (i == 30 & iniManager.IsThereSection("Interface Tweaks"))
                {
                    tempSection.Clear(); tempKeys.Clear();

                    tempSection = iniManager.GetSection("Interface Tweaks");
                    tempKeys = iniManager.GetKeys("Interface Tweaks");

                    var acceptanceList = tempSection.Zip(tempKeys, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700);
                        Parallel.Invoke(() => InterfaceTweaks.UseInterface(set.Tweak, Convert.ToBoolean(set.Value)));

                        switch (set.Tweak)
                        {
                            case "TglButton1":
                            case "TglButton2":
                            case "TglButton3":
                            case "TglButton4":
                            case "TglButton5":
                            case "TglButton10":
                            case "TglButton11":
                            case "TglButton12":
                            case "TglButton26":
                            case "TglButton27":
                                isLogoutNeed = true;
                                break;
                            case "TglButton22":
                                isRestartNeed = true;
                                break;
                        }
                    }
                }

                if (i == 50 & iniManager.IsThereSection("Services Tweaks"))
                {
                    tempSection.Clear(); tempKeys.Clear();

                    tempSection = iniManager.GetSection("Services Tweaks");
                    tempKeys = iniManager.GetKeys("Services Tweaks");

                    var acceptanceList = tempSection.Zip(tempKeys, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700);
                        Parallel.Invoke(() => ServicesTweaks.UseServices(set.Tweak, Convert.ToBoolean(set.Value)));
                        isRestartNeed = true;
                    }
                }

                if (i == 80 & iniManager.IsThereSection("System Tweaks"))
                {
                    tempSection.Clear(); tempKeys.Clear();

                    tempSection = iniManager.GetSection("System Tweaks");
                    tempKeys = iniManager.GetKeys("System Tweaks");

                    var acceptanceList = tempSection.Zip(tempKeys, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700);

                        if (set.Tweak != "TglButton8")
                        {
                            if (set.Tweak.Contains("TglButton"))
                                Parallel.Invoke(() => SystemTweaks.UseSystem(set.Tweak, Convert.ToBoolean(set.Value)));
                            else
                                SystemTweaks.UseSystemSliders(set.Tweak, Convert.ToUInt32(set.Value));

                            switch (set.Tweak)
                            {
                                case "TglButton7":
                                case "TglButton9":
                                case "TglButton10":
                                case "TglButton12":
                                case "TglButton13":
                                case "TglButton14":
                                case "TglButton15":
                                case "TglButton20":
                                    isRestartNeed = true;
                                    break;
                                case "TglButton2":
                                    isLogoutNeed = true;
                                    break;
                            }
                        }
                        else
                        {
                            if (!SystemTweaks.isTweakWorkingAntivirus)
                            {
                                SystemTweaks.isTweakWorkingAntivirus = true;
                                SystemTweaks.UseSystem(set.Tweak, Convert.ToBoolean(set.Value));
                                await Task.Delay(20000);
                            }
                        }
                    }
                }

                await Task.Delay(1);

                _progress.Report(i);
            }
        }

    }
}
