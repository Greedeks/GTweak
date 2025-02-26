using GTweak.Utilities.Helpers;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace GTweak
{
    public partial class App : Application
    {
        internal static event EventHandler LanguageChanged;
        internal static event EventHandler ThemeChanged;
        internal static event EventHandler ImportTweaksUpdate;

        internal static string GettingSystemLanguage => Regex.Replace(CultureInfo.CurrentCulture.ToString(), @"-.+$", "", RegexOptions.Multiline);

        public App()
        {
            InitializeComponent();
        }

        internal static void UpdateImport() => ImportTweaksUpdate?.Invoke(default, EventArgs.Empty);

        internal static void ViewingSettings()
        {
            Language = RegistryHelp.GetValue(@"HKEY_CURRENT_USER\Software\GTweak", "Language", GettingSystemLanguage);
            Theme = RegistryHelp.GetValue(@"HKEY_CURRENT_USER\Software\GTweak", "Theme", "Dark");
        }

        internal static string Language
        {
            set
            {
                value ??= GettingSystemLanguage;

                ResourceDictionary dictionary = new ResourceDictionary
                {
                    Source = value switch
                    {
                        "en" => new Uri("Languages/en/Localize.xaml", UriKind.Relative),
                        "ko" => new Uri("Languages/ko/Localize.xaml", UriKind.Relative),
                        "ru" => new Uri("Languages/ru/Localize.xaml", UriKind.Relative),
                        "uk" => new Uri("Languages/uk/Localize.xaml", UriKind.Relative),
                        _ => GettingSystemLanguage switch
                        {
                            string lang when lang.Contains("ko") => new Uri("Languages/ko/Localize.xaml", UriKind.Relative),
                            string lang when lang.Contains("ru") => new Uri("Languages/ru/Localize.xaml", UriKind.Relative),
                            string lang when lang.Contains("uk") => new Uri("Languages/uk/Localize.xaml", UriKind.Relative),
                            _ => new Uri("Languages/en/Localize.xaml", UriKind.Relative)
                        }
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
                        "Light" => new Uri($"Styles/Themes/Light/Colors.xaml", UriKind.Relative),
                        "Dark" => new Uri($"Styles/Themes/Dark/Colors.xaml", UriKind.Relative),
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
