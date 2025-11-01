namespace GTweak.Core.Model
{
    internal sealed class MainWinModel
    {
        public object CurrentView { get; set; }
        public string SelectedLanguage { get; set; }

        internal class LanguageItem
        {
            public string Code { get; set; }
            public string Display { get; set; }
        }
    }
}
