﻿using GTweak.Utilities;
using GTweak.Utilities.Tweaks;
using System;
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
            for (int i = 1; i <= 100; i++)
            {
                _token.ThrowIfCancellationRequested();
                СonfigSettings configSettings = new СonfigSettings(Settings.PathConfig);

                if (i == 2 && configSettings.IsThereSection("Confidentiality Tweaks"))
                {
                    for (byte number = 1; number <= 18; number++)
                    {
                        string value = configSettings.ReadConfig("Confidentiality Tweaks", "TglButton" + number);
                        if (!string.IsNullOrEmpty(value))
                        {
                            await Task.Delay(700);
                            Parallel.Invoke(() => ConfidentialityTweaks.UseСonfidentiality("TglButton" + number, Convert.ToBoolean(value)));
                            switch (number)
                            {
                                case 8:
                                case 15:
                                    isRestartNeed = true;
                                    break;
                            }
                        }
                    }
                }

                if (i == 30 && configSettings.IsThereSection("Interface Tweaks"))
                {
                    for (byte number = 1; number <= 28; number++)
                    {
                        string value = configSettings.ReadConfig("Interface Tweaks", "TglButton" + number);
                        if (!string.IsNullOrEmpty(value))
                        {
                            await Task.Delay(700);
                            Parallel.Invoke(() => InterfaceTweaks.UseInterface("TglButton" + number, Convert.ToBoolean(value)));

                            switch (number)
                            {
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 10:
                                case 11:
                                case 12:
                                case 26:
                                case 27:
                                    isLogoutNeed = true;
                                    break;
                                case 22:
                                    isRestartNeed = true;
                                    break;
                            }
                        }
                    }
                }

                if (i == 50 && configSettings.IsThereSection("Services Tweaks"))
                {
                    for (byte number = 1; number <= 28; number++)
                    {
                        string value = configSettings.ReadConfig("Services Tweaks", "TglButton" + number);
                        if (!string.IsNullOrEmpty(value))
                        {
                            await Task.Delay(700);
                            Parallel.Invoke(() => ServicesTweaks.UseServices("TglButton" + number, Convert.ToBoolean(value)));
                            isRestartNeed = true;
                        }
                    }
                }

                if (i == 80 && configSettings.IsThereSection("System Tweaks"))
                {
                    for (byte number = 1; number <= 3; number++)
                    {
                        string value = configSettings.ReadConfig("System Tweaks", "Slider" + number);

                        if (!string.IsNullOrEmpty(value))
                        {
                            await Task.Delay(700);
                            SystemTweaks.UseSystemSliders("Slider" + number, Convert.ToUInt32(value));
                        }

                    }
                    for (byte number = 1; number <= 23; number++)
                    {
                        string value = configSettings.ReadConfig("System Tweaks", "TglButton" + number);

                        if (!string.IsNullOrEmpty(value))
                        {
                            await Task.Delay(700);

                            if ("TglButton" + number != "TglButton8")
                            {
                                Parallel.Invoke(() => SystemTweaks.UseSystem("TglButton" + number, Convert.ToBoolean(value)));
                                new ViewNotification().Show("restart");

                                switch (number)
                                {
                                    case 7:
                                    case 8:
                                    case 9:
                                    case 10:
                                    case 12:
                                    case 13:
                                    case 14:
                                    case 15:
                                    case 20:
                                        isRestartNeed = true;
                                        break;
                                    case 2:
                                        isLogoutNeed = true;
                                        break;
                                }
                            }
                            else
                            {
                                if (!SystemTweaks.isTweakWorkingAntivirus)
                                {
                                    SystemTweaks.isTweakWorkingAntivirus = true;
                                    SystemTweaks.UseSystem("TglButton" + number, Convert.ToBoolean(value));
                                    await Task.Delay(20000);
                                }
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
