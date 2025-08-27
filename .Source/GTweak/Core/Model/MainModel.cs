namespace GTweak.Core.Model
{
    internal class MainModel
    {
        public object CurrentView { get; set; }
        public string SelectedLanguage { get; set; }
        public string SelectedTheme { get; set; }

        internal class LanguageItem
        {
            public string Code { get; set; }
            public string Display { get; set; }
        }

        internal class ThemeItem
        {
            public string Code { get; set; }
            public string Display { get; set; }
        }
    }
}
