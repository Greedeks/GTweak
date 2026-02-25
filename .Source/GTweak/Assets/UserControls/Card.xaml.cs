using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GTweak.Assets.UserControls
{
    public partial class Card
    {
        internal event EventHandler ClickButton, ClickButtonSecondary;

        internal Style IconStyle { get => (Style)GetValue(IconStyleProperty); set => SetValue(IconStyleProperty, value); }

        internal DynamicResourceExtension Title { set { if (value != null) { CardTitle?.SetResourceReference(TextBlock.TextProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); } } }

        internal DynamicResourceExtension Description { set { if (value != null) { CardText?.SetResourceReference(TextBlock.TextProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); } } }

        internal DynamicResourceExtension BtnContent { set { if (value != null) { CardButton?.SetResourceReference(ContentProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); } } }

        internal DynamicResourceExtension BtnContentSecondary
        {
            set
            {
                if (CardButtonSecondary != null)
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
        }

        internal static readonly DependencyProperty IconStyleProperty =
            DependencyProperty.Register(nameof(IconStyle), typeof(Style), typeof(Card), new PropertyMetadata(null, OnIconStyleChanged));

        internal static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(TextBlock), typeof(string), typeof(Card), new PropertyMetadata(string.Empty));

        internal static new readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(BtnContent), typeof(string), typeof(Button), new PropertyMetadata(string.Empty, (s, e) => { if (s is Button btn && e.NewValue is string str) { btn.Content = str; } }));

        private static void OnIconStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Card card && card.CardIcon != null)
            {
                if (e.NewValue is Style style)
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