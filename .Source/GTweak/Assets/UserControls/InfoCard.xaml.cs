using System.Windows;
using System.Windows.Controls;

namespace GTweak.Assets.UserControls
{
    public partial class InfoCard : UserControl
    {

        internal static readonly DependencyProperty IconStyleProperty =
            DependencyProperty.Register(nameof(IconStyle), typeof(Style), typeof(InfoCard), new PropertyMetadata(null, OnIconStyleChanged));

        internal static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(InfoCard));

        internal static readonly DependencyProperty CardContentProperty =
            DependencyProperty.Register(nameof(CardContent), typeof(object), typeof(InfoCard));

        internal Style IconStyle
        {
            get => (Style)GetValue(IconStyleProperty);
            set => SetValue(IconStyleProperty, value);
        }

        internal string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        internal object CardContent
        {
            get => GetValue(CardContentProperty);
            set => SetValue(CardContentProperty, value);
        }

        private static void OnIconStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InfoCard card && card.CardIcon != null)
            {
                if (e.NewValue is Style style)
                {
                    card.CardIcon.Style = style;
                }
            }
        }

        public InfoCard()
        {
            InitializeComponent();
        }
    }
}
