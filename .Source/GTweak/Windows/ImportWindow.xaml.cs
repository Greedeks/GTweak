using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Helpers.Storage;
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
        private bool isRestartNeed = false, isLogoutNeed = false, isExpRestartNeed = false;
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
                if (isExpRestartNeed)
                    ExplorerManager.Restart(new Process());

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

            INIManager iniManager = new INIManager(StoragePaths.Config);

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

                        isRestartNeed = NotifActionsStorage.GetConfActions.Any(get => get.Key == set.Tweak && get.Value == "restart");
                        isLogoutNeed = NotifActionsStorage.GetConfActions.Any(get => get.Key == set.Tweak && get.Value == "logout");
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
                        await Task.Delay(700, _token);
                        InterfaceTweaks.ApplyTweaks(set.Tweak, Convert.ToBoolean(set.Value));

                        isRestartNeed = NotifActionsStorage.GetIntfActions.Any(get => get.Key == set.Tweak && get.Value == "restart");
                        isLogoutNeed = NotifActionsStorage.GetIntfActions.Any(get => get.Key == set.Tweak && get.Value == "logout");
                        isExpRestartNeed = ExplorerManager.GetIntfStorage.Any(get => get.Key == set.Tweak && get.Value == true);
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

                            isRestartNeed = NotifActionsStorage.GetSysActions.Any(get => get.Key == set.Tweak && get.Value == "restart");
                            isLogoutNeed = NotifActionsStorage.GetSysActions.Any(get => get.Key == set.Tweak && get.Value == "logout");
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

