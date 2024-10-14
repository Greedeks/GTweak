using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GTweak.Assets.UserControl
{
    public partial class Card
    {
        /// <summary>
        /// Custom Event - Processing keypresses only for the button
        /// </summary>
        internal event EventHandler ClickButton;

        internal ImageSource ImageSource { get => CardImage.Source; set { CardImage.Source = value; } }
        internal DynamicResourceExtension Title { set => CardTitle.SetResourceReference(TextBlock.TextProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); }
        internal DynamicResourceExtension Description { set => CardText.SetResourceReference(TextBlock.TextProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); }
        internal DynamicResourceExtension BtnContent { set => CardButton.SetResourceReference(ContentProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); }

        internal static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("DescriptionBlock", typeof(string), typeof(TextBlock), new PropertyMetadata("", (s, e) => ((TextBlock)s).Text = (string)e.NewValue));

        internal new static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("ContentButton", typeof(string), typeof(Button), new PropertyMetadata("", (s, e) => ((Button)s).Content = (string)e.NewValue));

        public Card()
        {
            InitializeComponent();
        }

        private void CardButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && ClickButton != null)
            {
                ClickButton(this, EventArgs.Empty);
            }
        }
    }
}
