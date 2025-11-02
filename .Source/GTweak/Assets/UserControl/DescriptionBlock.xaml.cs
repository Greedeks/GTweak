using System;
using System.Windows;
using GTweak.Utilities.Animation;

namespace GTweak.Assets.UserControl
{
    public partial class DescriptionBlock
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
                TypewriterAnimation.Create(value, FunctionDescription,
                value.Length <= 50 ? TimeSpan.FromMilliseconds(200) : value.Length <= 200 ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(550));
            }
        }

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(DescriptionBlock), new UIPropertyMetadata(string.Empty));


        public DescriptionBlock()
        {
            InitializeComponent();
            App.LanguageChanged += delegate { TypewriterAnimation.Create(DefaultText, FunctionDescription, TimeSpan.FromMilliseconds(0)); };
        }

        private void DescriptionBlock_Loaded(object sender, RoutedEventArgs e) => TypewriterAnimation.Create(DefaultText, FunctionDescription, TimeSpan.FromMilliseconds(300));
    }
}

