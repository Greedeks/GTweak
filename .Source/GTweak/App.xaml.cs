﻿using Microsoft.Win32;
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

        internal static string Language
        {
            set
            {
                value ??= Regex.Replace(CultureInfo.CurrentCulture.ToString(), @"-.+$", "", RegexOptions.Multiline).ToString();

                ResourceDictionary dictionary = new ResourceDictionary
                {
                    Source = value switch
                    {
                        "ru" => new Uri($"Language/lang.ru.xaml", UriKind.Relative),
                        _ => new Uri("Language/lang.xaml", UriKind.Relative),
                    }
                };

                ResourceDictionary oldDictionary = (from dict in Current.Resources.MergedDictionaries
                where dict.Source != null && dict.Source.OriginalString.StartsWith("Language/lang.") select dict).First();
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

        internal static void ViewLang() => Language = Registry.CurrentUser.OpenSubKey(@"Software\GTweak")?.GetValue("Language")?.ToString() ?? Regex.Replace(CultureInfo.CurrentCulture.ToString(), @"-.+$", "", RegexOptions.Multiline).ToString();
        
     

    }
}
