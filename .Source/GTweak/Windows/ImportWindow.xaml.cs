using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
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
        private struct SectionName
        {
            internal const string Confidentiality = "Confidentiality Tweaks";
            internal const string Interface = "Interface Tweaks";
            internal const string Services = "Services Tweaks";
            internal const string System = "System Tweaks";
        }

        private bool isRestartNeed = false, isLogoutNeed = false;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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
            Progress<byte> progress = new Progress<byte>(ReportProgress);
            try { await SortByPageDate(_cancellationTokenSource.Token, progress); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }

        private void ReportProgress(byte valueProgress)
        {
            if (valueProgress == 100)
            {
                if (isRestartNeed)
                    new ViewNotification().Show("restart");
                else if (!isRestartNeed && isLogoutNeed)
                    new ViewNotification().Show("logout");
                App.UpdateImport();
                Close();
            }
        }

        private async Task SortByPageDate(CancellationToken _token, IProgress<byte> _progress)
        {
            List<string> tempKeys = new List<string>(), tempValues = new List<string>();

            INIManager iniManager = new INIManager(UsePath.Config);

            for (byte i = 1; i <= 100; i++)
            {
                _token.ThrowIfCancellationRequested();

                if (i == 2 & iniManager.IsThereSection(SectionName.Confidentiality))
                {
                    tempKeys.Clear(); tempValues.Clear();

                    tempKeys = iniManager.GetKeysOrValue(SectionName.Confidentiality);
                    tempValues = iniManager.GetKeysOrValue(SectionName.Confidentiality, false);

                    var acceptanceList = tempKeys.Zip(tempValues, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700, _token);
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

                if (i == 30 & iniManager.IsThereSection(SectionName.Interface))
                {
                    tempKeys.Clear(); tempValues.Clear();

                    tempKeys = iniManager.GetKeysOrValue(SectionName.Interface);
                    tempValues = iniManager.GetKeysOrValue(SectionName.Interface, false);

                    var acceptanceList = tempKeys.Zip(tempValues, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700, _token);
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
                            case "TglButton20":
                                isRestartNeed = true;
                                break;
                        }
                    }
                }

                if (i == 50 & iniManager.IsThereSection(SectionName.Services))
                {
                    tempKeys.Clear(); tempValues.Clear();

                    tempKeys = iniManager.GetKeysOrValue(SectionName.Services);
                    tempValues = iniManager.GetKeysOrValue(SectionName.Services, false);

                    var acceptanceList = tempKeys.Zip(tempValues, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700, _token);
                        Parallel.Invoke(() => ServicesTweaks.UseServices(set.Tweak, Convert.ToBoolean(set.Value)));
                        isRestartNeed = true;
                    }
                }

                if (i == 80 & iniManager.IsThereSection(SectionName.System))
                {
                    tempKeys.Clear(); tempValues.Clear();

                    tempKeys = iniManager.GetKeysOrValue(SectionName.System);
                    tempValues = iniManager.GetKeysOrValue(SectionName.System, false);

                    var acceptanceList = tempKeys.Zip(tempValues, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700, _token);

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
                                await Task.Delay(20000, _token);
                            }
                        }
                    }
                }

                await Task.Delay(1, _token);

                _progress.Report(i);
            }
        }

    }
}

