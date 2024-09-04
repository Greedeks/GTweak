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

        internal static void UpdateImport() => ImportTweaksUpdate?.Invoke(null, EventArgs.Empty);

        internal static string Language
        {
            set
            {
                value ??= Regex.Replace(CultureInfo.CurrentCulture.ToString(), @"-.+$", "", RegexOptions.Multiline);

                ResourceDictionary dictionary = new ResourceDictionary
                {
                    Source = value switch
                    {
                        "ru" => new Uri($"Language/Lang.RU.xaml", UriKind.Relative),
                        _ => new Uri("Language/Lang.xaml", UriKind.Relative),
                    }
                };

                ResourceDictionary oldDictionary = (from dict in Current.Resources.MergedDictionaries
                where dict.Source != null && dict.Source.OriginalString.StartsWith("Language/Lang.") select dict).First();
                if (oldDictionary != null)
                {
                    int ind = Current.Resources.MergedDictionaries.IndexOf(oldDictionary);
                    Current.Resources.MergedDictionaries.Remove(oldDictionary);
                    Current.Resources.MergedDictionaries.Insert(ind, dictionary);
                }
                else {Current.Resources.MergedDictionaries.Add(dictionary); }

                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        internal static void ViewingSettings()
        {
            Language = Registry.CurrentUser.OpenSubKey(@"Software\GTweak")?.GetValue("Language")?.ToString() ?? Regex.Replace(CultureInfo.CurrentCulture.ToString(), @"-.+$", "", RegexOptions.Multiline);
            Theme = Registry.CurrentUser.OpenSubKey(@"Software\GTweak")?.GetValue("Theme")?.ToString() ?? "Dark";
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
                        "Light" => new Uri($"Styles/Theme/Colors.Light.xaml", UriKind.Relative),
                        _ => new Uri("Styles/Theme/Colors.xaml", UriKind.Relative),
                    }
                };

                ResourceDictionary oldDictionary = (from dict in Current.Resources.MergedDictionaries
                                                    where dict.Source != null && dict.Source.OriginalString.StartsWith("Styles/Theme/Colors.")
                                                    select dict).First();
                if (oldDictionary != null)
                {
                    int ind = Current.Resources.MergedDictionaries.IndexOf(oldDictionary);
                    Current.Resources.MergedDictionaries.Remove(oldDictionary);
                    Current.Resources.MergedDictionaries.Insert(ind, dictionary);
                }
                else { Current.Resources.MergedDictionaries.Add(dictionary); }

                ThemeChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
}
