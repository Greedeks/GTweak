using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace GTweak.Utilities.Animation
{
    internal static class NumberAnimation
    {
        internal static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached("Value", typeof(int), typeof(NumberAnimation), new PropertyMetadata(0, OnValueChanged));

        private static readonly DependencyProperty AnimatedValueProperty = DependencyProperty.RegisterAttached("AnimatedValue", typeof(int), typeof(NumberAnimation), new PropertyMetadata(0, OnAnimatedValueChanged));

        internal static void SetValue(DependencyObject dObject, int value) => dObject.SetValue(ValueProperty, value);

        internal static int GetValue(DependencyObject dObject) => (int)dObject.GetValue(ValueProperty);

        private static void SetAnimatedValue(DependencyObject dObject, int value) => dObject.SetValue(AnimatedValueProperty, value);

        private static void OnValueChanged(DependencyObject dObject, DependencyPropertyChangedEventArgs e)
        {
            if (dObject is TextBlock textBlock)
            {
                int newValue = (int)e.NewValue;

                if (!textBlock.IsLoaded)
                {
                    SetAnimatedValue(textBlock, newValue);
                    return;
                }

                int oldValue = (int)e.OldValue;

                Int32Animation int32Anim = new Int32Animation
                {
                    From = oldValue,
                    To = newValue,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                Timeline.SetDesiredFrameRate(int32Anim, 60);
                textBlock.BeginAnimation(AnimatedValueProperty, int32Anim);
            }
        }

        private static void OnAnimatedValueChanged(DependencyObject dObject, DependencyPropertyChangedEventArgs e)
        {
            if (dObject is TextBlock textBlock)
                textBlock.Text = string.Format(" {0}", e.NewValue.ToString());
        }
    }
}
