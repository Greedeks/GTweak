using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Windows;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;

namespace GTweak
{
    public partial class App : Application
    {
        internal static event EventHandler LanguageChanged;
        internal static event EventHandler ThemeChanged;
        internal static event EventHandler ImportTweaksUpdate;

        internal static string GettingSystemLanguage => Regex.Replace(CultureInfo.CurrentCulture.ToString(), @"-.+$", "", RegexOptions.Multiline);
        internal static void UpdateImport() => ImportTweaksUpdate?.Invoke(default, EventArgs.Empty);
        private void LogError(Exception ex, [CallerMemberName] string memberName = "") => ErrorLogging.LogWritingFile(ex, memberName);

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
                SettingsRepository.SelfRemoval();
                return;
            }

            Language = RegistryHelp.GetValue(@"HKEY_CURRENT_USER\Software\GTweak", "Language", "en");
            Theme = RegistryHelp.GetValue(@"HKEY_CURRENT_USER\Software\GTweak", "Theme", "Dark");

            BlockRunTweaker.CheckingApplicationCopies();
            await BlockRunTweaker.CheckingSystemRequirements();

            new LoadingWindow().Show();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogError(e.Exception);
            e.Handled = true;
            Environment.Exit(0);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                LogError(ex);
            Environment.Exit(0);
        }

        internal static string Language
        {
            set
            {
                value ??= GettingSystemLanguage;

                ResourceDictionary dictionary = new ResourceDictionary
                {
                    Source = (value == "be" ? "ru" : value) switch
                    {
                        "fr" => new Uri("Languages/fr/Localize.xaml", UriKind.Relative),
                        "it" => new Uri("Languages/it/Localize.xaml", UriKind.Relative),
                        "ko" => new Uri("Languages/ko/Localize.xaml", UriKind.Relative),
                        "ru" => new Uri("Languages/ru/Localize.xaml", UriKind.Relative),
                        "uk" => new Uri("Languages/uk/Localize.xaml", UriKind.Relative),
                        _ => new Uri("Languages/en/Localize.xaml", UriKind.Relative),
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
                        "Dark" => new Uri($"Styles/Themes/Dark/Colors.xaml", UriKind.Relative),
                        "Light" => new Uri($"Styles/Themes/Light/Colors.xaml", UriKind.Relative),
                        "Cobalt" => new Uri($"Styles/Themes/Cobalt/Colors.xaml", UriKind.Relative),
                        "Dark amethyst" => new Uri($"Styles/Themes/Dark amethyst/Colors.xaml", UriKind.Relative),
                        "Cold Blue" => new Uri($"Styles/Themes/Cold blue/Colors.xaml", UriKind.Relative),
                        _ => RegistryHelp.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", string.Empty) == "0" ? new Uri("Styles/Themes/Dark/Colors.xaml", UriKind.Relative) : new Uri($"Styles/Themes/Light/Colors.xaml", UriKind.Relative),
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
