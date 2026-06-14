using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GTweak.Utilities.Animation;

namespace GTweak.Core.Behaviors
{
    internal class ArcAnimationBehavior
    {
        internal static readonly DependencyProperty AnimatedAngleProperty =
            DependencyProperty.RegisterAttached("AnimatedAngle", typeof(double), typeof(ArcAnimationBehavior), new PropertyMetadata(0.0, OnAngleChanged));

        internal static void SetAnimatedAngle(DependencyObject obj, double value) => obj.SetValue(AnimatedAngleProperty, value);
        internal static double GetAnimatedAngle(DependencyObject obj) => (double)obj.GetValue(AnimatedAngleProperty);

        private static void OnAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Wpf.Ui.Controls.Arc arc)
            {
                arc.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    DoubleAnimation animation = FactoryAnimation.CreateIn(arc.EndAngle, (double)e.NewValue, 0.2, useCubicEase: true);
                    Timeline.SetDesiredFrameRate(animation, 60);
                    arc.BeginAnimation(Wpf.Ui.Controls.Arc.EndAngleProperty, animation);
                }));
            }
        }
    }
}
