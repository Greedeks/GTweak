using System.Windows;
using System.Windows.Media.Effects;

namespace GTweak.Utilities.Animation
{
    internal class BlurAnimation
    {
        internal static readonly DependencyProperty TargetRadiusProperty =
            DependencyProperty.RegisterAttached("TargetRadius", typeof(double), typeof(BlurAnimation), new PropertyMetadata(0.0, OnTargetRadiusChanged));

        internal static double GetTargetRadius(DependencyObject obj) => (double)obj.GetValue(TargetRadiusProperty);
        internal static void SetTargetRadius(DependencyObject obj, double value) => obj.SetValue(TargetRadiusProperty, value);

        private static void OnTargetRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (!(element.Effect is BlurEffect blurEffect))
                {
                    blurEffect = new BlurEffect { Radius = (double)e.OldValue };
                    element.Effect = blurEffect;
                }

                double fromValue = (double)e.OldValue;
                double toValue = (double)e.NewValue;

                blurEffect.BeginAnimation(BlurEffect.RadiusProperty, FactoryAnimation.CreateIn(fromValue, toValue, 0.2));
            }
        }
    }
}
