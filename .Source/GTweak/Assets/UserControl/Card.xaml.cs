﻿using System;
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

        internal ImageSource ImageSource { set => SetValue(ImageSourceProperty, value); }
        internal DynamicResourceExtension Title { set => CardTitle.SetResourceReference(TextBlock.TextProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); }
        internal DynamicResourceExtension Description { set => CardText.SetResourceReference(TextBlock.TextProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); }
        internal DynamicResourceExtension BtnContent { set => CardButton.SetResourceReference(ContentProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); }

        internal static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("DescriptionBlock", typeof(string), typeof(Card), new PropertyMetadata(string.Empty));

        internal static new readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("BtnContent", typeof(string), typeof(Button), new PropertyMetadata(string.Empty, (s, e) => ((Button)s).Content = (string)e.NewValue));

        internal static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(Card), new PropertyMetadata(default(ImageSource)));

        public Card()
        {
            InitializeComponent();
        }

        private void CardButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => ClickButton?.Invoke(this, EventArgs.Empty);
    }
}
