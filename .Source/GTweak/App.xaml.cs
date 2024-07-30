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
        internal static event EventHandler ImportTweaksUpdate;
        public App()
        {
            InitializeComponent();
        }

        internal static CultureInfo Language
        {
            get => System.Threading.Thread.CurrentThread.CurrentUICulture;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                ResourceDictionary dictionary = new ResourceDictionary();

                string getSystemLanguage = Regex.Replace(value.Name, @"-.+$", "",RegexOptions.Multiline);

                dictionary.Source = getSystemLanguage switch
                {
                    "ru" => new Uri($"Language/lang.ru.xaml", UriKind.Relative),
                    _ => new Uri("Language/lang.xaml", UriKind.Relative),
                };

                ResourceDictionary oldDictionary = (from d in Current.Resources.MergedDictionaries
                where d.Source != null && d.Source.OriginalString.StartsWith("Language/lang.") select d).First();
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

        internal static void UpdateImport()=> ImportTweaksUpdate?.Invoke(null, EventArgs.Empty);

        internal static void ViewLang() => Language = Registry.CurrentUser.OpenSubKey(@"Software\GTweak")?.GetValue("Language")?.ToString() == "en" ? CultureInfo.GetCultureInfo("en-US") : CultureInfo.GetCultureInfo("ru-RU");
     

    }
}
