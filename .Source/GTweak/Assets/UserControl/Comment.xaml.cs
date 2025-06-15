using GTweak.Utilities.Animation;
using System;
using System.Windows;

namespace GTweak.Assets.UserControl
{
    public partial class Comment
    {
        internal string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        internal string Text
        {
            get { return FunctionDescription.Text; }
            set
            {
                new TypewriterAnimation(value, FunctionDescription,
                value.Length <= 50 ? TimeSpan.FromMilliseconds(200) : value.Length <= 200 ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(550));
            }
        }

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(Comment), new UIPropertyMetadata(string.Empty));

        public Comment()
        {
            InitializeComponent();

            App.LanguageChanged += delegate { new TypewriterAnimation(DefaultText, FunctionDescription, TimeSpan.FromMilliseconds(0)); };
        }

        private void Comment_Loaded(object sender, RoutedEventArgs e) => new TypewriterAnimation(DefaultText, FunctionDescription, TimeSpan.FromMilliseconds(300));
    }
}
