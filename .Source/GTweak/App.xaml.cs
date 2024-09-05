using Microsoft.Win32;
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

        public App()
        {
            InitializeComponent();
        }

        internal static void UpdateImport() => ImportTweaksUpdate?.Invoke(default, EventArgs.Empty);

        internal static void ViewingSettings()
        {
            Language = Registry.CurrentUser.OpenSubKey(@"Software\GTweak")?.GetValue("Language")?.ToString() ?? Regex.Replace(CultureInfo.CurrentCulture.ToString(), @"-.+$", "", RegexOptions.Multiline);
            Theme = Registry.CurrentUser.OpenSubKey(@"Software\GTweak")?.GetValue("Theme")?.ToString() ?? "Dark";
        }

        internal static string Language
        {
            set
            {
                value ??= Regex.Replace(CultureInfo.CurrentCulture.ToString(), @"-.+$", "", RegexOptions.Multiline);

                ResourceDictionary dictionary = new ResourceDictionary
                {
                    Source = value switch
                    {
                        "ru" => new Uri($"Languages/ru/Lang.xaml", UriKind.Relative),
                        _ => new Uri("Languages/en/Lang.xaml", UriKind.Relative),
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
                        _ => Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")?.GetValue("AppsUseLightTheme")?.ToString()! == "0" ? new Uri("Styles/Themes/Dark/Colors.xaml", UriKind.Relative) : new Uri($"Styles/Themes/Light/Colors.xaml", UriKind.Relative),
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
