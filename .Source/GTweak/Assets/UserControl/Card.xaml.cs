using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace GTweak.Assets.UserControl
{
    public partial class Card
    {
        internal event EventHandler ClickButton;
        internal event EventHandler ClickButtonSecondary;

        internal Style IconStyle { get => (Style)GetValue(IconStyleProperty); set => SetValue(IconStyleProperty, value); }
        internal DynamicResourceExtension Title { set => CardTitle.SetResourceReference(TextBlock.TextProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); }
        internal DynamicResourceExtension Description { set => CardText.SetResourceReference(TextBlock.TextProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); }
        internal DynamicResourceExtension BtnContent { set => CardButton.SetResourceReference(ContentProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); }

        internal DynamicResourceExtension BtnContentSecondary
        {
            set
            {
                if (value != null)
                {
                    CardButtonSecondary.SetResourceReference(ContentProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey));
                    CardButtonSecondary.Visibility = Visibility.Visible;
                }
                else
                {
                    CardButtonSecondary.Visibility = Visibility.Collapsed;
                }
            }
        }

        internal static readonly DependencyProperty IconStyleProperty =
            DependencyProperty.Register(nameof(IconStyle), typeof(Style), typeof(Card), new PropertyMetadata(null, OnIconStyleChanged));

        internal static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(TextBlock), typeof(string), typeof(Card), new PropertyMetadata(string.Empty));

        internal static new readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(BtnContent), typeof(string), typeof(Button), new PropertyMetadata(string.Empty, (s, e) => ((Button)s).Content = (string)e.NewValue));

        private static void OnIconStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Card card && card.CardIcon != null)
            {
                if (e.NewValue is Style style && style != null)
                {
                    card.CardIcon.Style = style;
                }
            }
        }

        public Card()
        {
            InitializeComponent();
        }

        private void CardButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => ClickButton?.Invoke(this, EventArgs.Empty);

        private void CardButtonSecondary_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => ClickButtonSecondary?.Invoke(this, EventArgs.Empty);
    }
}
