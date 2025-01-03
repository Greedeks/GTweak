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
            try { await SortByPageDate(_cancellationTokenSource.Token, progress); } catch (Exception ex) { Debug.WriteLine(ex.Message); }
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

                if (i == 2 & iniManager.IsThereSection(INIManager.SectionConf))
                {
                    tempKeys.Clear(); tempValues.Clear();

                    tempKeys = iniManager.GetKeysOrValue(INIManager.SectionConf);
                    tempValues = iniManager.GetKeysOrValue(INIManager.SectionConf, false);

                    var acceptanceList = tempKeys.Zip(tempValues, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700, _token);
                        ConfidentialityTweaks.ApplyTweaks(set.Tweak, Convert.ToBoolean(set.Value));

                        switch (set.Tweak)
                        {
                            case "TglButton8":
                            case "TglButton15":
                                isRestartNeed = true;
                                break;
                        }
                    }
                }

                if (i == 30 & iniManager.IsThereSection(INIManager.SectionIntf))
                {
                    tempKeys.Clear(); tempValues.Clear();

                    tempKeys = iniManager.GetKeysOrValue(INIManager.SectionIntf);
                    tempValues = iniManager.GetKeysOrValue(INIManager.SectionIntf, false);

                    var acceptanceList = tempKeys.Zip(tempValues, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(set.Tweak switch
                        {
                            "TglButton7" => 2000,
                            "TglButton8" => 2000,
                            "TglButton17" => 2000,
                            _ => 700,
                        }, _token);
                        InterfaceTweaks.ApplyTweaks(set.Tweak, Convert.ToBoolean(set.Value));

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

                if (i == 50 & iniManager.IsThereSection(INIManager.SectionSvc))
                {
                    tempKeys.Clear(); tempValues.Clear();

                    tempKeys = iniManager.GetKeysOrValue(INIManager.SectionSvc);
                    tempValues = iniManager.GetKeysOrValue(INIManager.SectionSvc, false);

                    var acceptanceList = tempKeys.Zip(tempValues, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700, _token);
                        ServicesTweaks.ApplyTweaks(set.Tweak, Convert.ToBoolean(set.Value));
                        isRestartNeed = true;
                    }
                }

                if (i == 80 & iniManager.IsThereSection(INIManager.SectionSys))
                {
                    tempKeys.Clear(); tempValues.Clear();

                    tempKeys = iniManager.GetKeysOrValue(INIManager.SectionSys);
                    tempValues = iniManager.GetKeysOrValue(INIManager.SectionSys, false);

                    var acceptanceList = tempKeys.Zip(tempValues, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700, _token);

                        if (set.Tweak != "TglButton8")
                        {
                            if (set.Tweak.Contains("TglButton"))
                                SystemTweaks.ApplyTweaks(set.Tweak, Convert.ToBoolean(set.Value));
                            else
                                SystemTweaks.ApplyTweaksSlider(set.Tweak, Convert.ToUInt32(set.Value));

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
                                SystemTweaks.ApplyTweaks(set.Tweak, Convert.ToBoolean(set.Value));
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

