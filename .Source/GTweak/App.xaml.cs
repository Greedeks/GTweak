using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Windows;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace GTweak
{
    public partial class App : Application
    {
        internal static event EventHandler LanguageChanged;
        internal static event EventHandler ThemeChanged;
        internal static event EventHandler ImportTweaksUpdate;

        internal static void UpdateImport() => ImportTweaksUpdate?.Invoke(default, EventArgs.Empty);

        public App()
        {
            InitializeComponent();

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private async void App_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Any(arg => arg.Equals("uninstall", StringComparison.OrdinalIgnoreCase)))
            {
                SettingsEngine.SelfRemoval();
                return;
            }

            SettingsEngine.СheckingParameters();
            RunGuard.CheckingApplicationCopies();
            await RunGuard.CheckingSystemRequirements();

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
                ErrorLogging.LogWritingFile(ex);
            Environment.Exit(0);
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
                        "fr" => GetResourceUri("fr"),
                        "it" => GetResourceUri("it"),
                        "ko" => GetResourceUri("ko"),
                        _ when code == "pt" && region == "br" => GetResourceUri("pt-br"),
                        _ when new[] { "ru", "be" }.Contains(value) => GetResourceUri("ru"),
                        "uk" => GetResourceUri("uk"),
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
                value ??= "Dark";

                ResourceDictionary dictionary = new ResourceDictionary
                {
                    Source = value switch
                    {
                        "Dark" => GetResourceUri("Dark", true),
                        "Light" => GetResourceUri("Light", true),
                        "Cobalt" => GetResourceUri("Cobalt", true),
                        "Dark amethyst" => GetResourceUri("Dark amethyst", true),
                        "Cold Blue" => GetResourceUri("Cold Blue", true),
                        _ => RegistryHelp.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", string.Empty) == "0" ? GetResourceUri("Dark", true) : GetResourceUri("Light", true)
                    }
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
    }
}
