using System;
using System.Globalization;
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
            if (e.Args != null && e.Args.Any(arg => string.Equals(arg ?? string.Empty, "uninstall", StringComparison.OrdinalIgnoreCase)))
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
            if (e != null)
            {
                ErrorLogging.LogWritingFile(e.Exception);
                e.Handled = true;
                Environment.Exit(0);
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e?.ExceptionObject is Exception ex)
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

                value ??= $"{code}-{region}";

                ResourceDictionary dictionary = new ResourceDictionary
                {
                    Source = value switch
                    {
                        "be" => GetResourceUri("ru"),
                        _ when SettingsEngine.AvailableLangs.Any(locale => string.Equals(locale, value, StringComparison.OrdinalIgnoreCase)) => GetResourceUri(value),
                        _ when value.Contains('-') && SettingsEngine.AvailableLangs.Any(locale => string.Equals(locale, value.Split('-')[0], StringComparison.OrdinalIgnoreCase)) => GetResourceUri(value.Split('-')[0]),
                        _ => GetResourceUri("en")
                    }
                };

                foreach (ResourceDictionary oldDictionary in Current.Resources.MergedDictionaries.Where(d => d.Source != null && d.Source.OriginalString.StartsWith("Languages/")).ToList())
                {
                    Current.Resources.MergedDictionaries.Remove(oldDictionary);
                }

                Current.Resources.MergedDictionaries.Add(dictionary);

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
                    Source = GetResourceUri(char.ToUpper(value[0], CultureInfo.InvariantCulture) + value.Substring(1).ToLower(CultureInfo.InvariantCulture), true)
                };

                foreach (ResourceDictionary oldDictionary in Current.Resources.MergedDictionaries.Where(d => d.Source != null && d.Source.OriginalString.StartsWith("Styles/Themes/")).ToList())
                {
                    Current.Resources.MergedDictionaries.Remove(oldDictionary);
                }

                Current.Resources.MergedDictionaries.Add(dictionary);

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

