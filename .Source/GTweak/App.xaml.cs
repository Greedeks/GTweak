using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Windows;

using Wpf.Ui.Appearance;

namespace GTweak
{
    public partial class App : Application
    {
        internal static event EventHandler LanguageChanged;
        internal static event EventHandler ThemeChanged;
        internal static event EventHandler TweaksImported;

        private readonly SystemDiagnostics _systemDiagnostics = new SystemDiagnostics();

        internal static void UpdateImport() => TweaksImported?.Invoke(default, EventArgs.Empty);

        public App()
        {
            InitializeComponent();

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            _systemDiagnostics.HandleDevicesEvents += OnHandleDevicesEvents;
        }

        private async void App_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Any(arg => arg.Equals("uninstall", StringComparison.OrdinalIgnoreCase)))
            {
                SettingsEngine.SelfRemoval();
                return;
            }

            SettingsEngine.СheckingParameters();
            ApplicationThemeManager.Apply(string.Equals(SettingsEngine.Theme, SettingsEngine.AvailableThemes.First(), StringComparison.OrdinalIgnoreCase) ? ApplicationTheme.Dark : ApplicationTheme.Light);

            RunGuard.CheckingApplicationCopies();
            await RunGuard.CheckingSystemRequirements();

            _systemDiagnostics.StartDeviceMonitoring();

            new LoadingWindow().Show();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ErrorLogging.LogWritingFile(e.Exception);
            e.Handled = true;
            Environment.Exit(0);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ErrorLogging.LogWritingFile(ex);
            }

            Environment.Exit(0);
        }

        private async void OnHandleDevicesEvents(MonitoringService.DeviceType deviceType)
        {
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate { _systemDiagnostics.UpdatingDevicesData(deviceType); });
        }

        private static Uri GetResourceUri(string folder, bool isTheme = false) => isTheme ? new Uri($"Styles/Themes/{folder}/Colors.xaml", UriKind.Relative) : new Uri($"Languages/{folder}/Localize.xaml", UriKind.Relative);

        internal static string Language
        {
            set
            {
                var (code, region) = SystemDiagnostics.GetCurrentSystemLang();

                value ??= code;

                if (value.Contains('-'))
                {
                    code = value.Split('-')[0];
                    region = value.Split('-').Length > 1 ? value.Split('-')[1] : string.Empty;
                }

                ResourceDictionary dictionary = new ResourceDictionary
                {
                    Source = value switch
                    {
                        "be" => GetResourceUri("ru"),
                        _ when code == "pt" && region == "br" => GetResourceUri("pt-br"),
                        _ when SettingsEngine.AvailableLangs?.Contains(value, StringComparer.OrdinalIgnoreCase) == true => GetResourceUri(value),
                        _ => GetResourceUri("en")
                    }
                };

                ResourceDictionary oldDictionary = (from dict in Current.Resources.MergedDictionaries
                                                    where dict.Source != null && dict.Source.OriginalString.StartsWith($"Languages/")
                                                    select dict).First();
                if (oldDictionary != null)
                {
                    int ind = Current.Resources.MergedDictionaries.IndexOf(oldDictionary);
                    Current.Resources.MergedDictionaries.Remove(oldDictionary);
                    Current.Resources.MergedDictionaries.Insert(ind, dictionary);
                }
                else { Current.Resources.MergedDictionaries.Add(dictionary); }

                LanguageChanged?.Invoke(default, EventArgs.Empty);
            }
        }

        internal static string Theme
        {
            set
            {
                value ??= SettingsEngine.AvailableThemes.First();

                ResourceDictionary dictionary = new ResourceDictionary
                {
                    Source = GetResourceUri(char.ToUpper(value[0]) + value.Substring(1).ToLower(), true)
                };

                ResourceDictionary oldDictionary = (from dict in Current.Resources.MergedDictionaries
                                                    where dict.Source != null && dict.Source.OriginalString.StartsWith("Styles/Themes/")
                                                    select dict).First();
                if (oldDictionary != null)
                {
                    int ind = Current.Resources.MergedDictionaries.IndexOf(oldDictionary);
                    Current.Resources.MergedDictionaries.Remove(oldDictionary);
                    Current.Resources.MergedDictionaries.Insert(ind, dictionary);
                }
                else { Current.Resources.MergedDictionaries.Add(dictionary); }

                ThemeChanged?.Invoke(default, EventArgs.Empty);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _systemDiagnostics?.StopDeviceMonitoring();
            base.OnExit(e);
        }
    }
}

