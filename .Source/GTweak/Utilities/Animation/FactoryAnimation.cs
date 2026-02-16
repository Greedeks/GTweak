using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace GTweak.Utilities.Animation
{
    internal static class FactoryAnimation
    {
        internal static DoubleAnimation CreateIn(double fromValue, double toValue, double seconds, Action onCompleted = null, bool reverse = false, bool useCubicEase = false)
        {
            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = fromValue,
                To = toValue,
                AutoReverse = reverse,
                Duration = TimeSpan.FromSeconds(seconds),
                EasingFunction = useCubicEase ? new CubicEase() : (IEasingFunction)new QuadraticEase()
            };

            if (onCompleted != null)
            {
                doubleAnim.Completed += delegate { onCompleted(); };
            }

            Timeline.SetDesiredFrameRate(doubleAnim, 120);
            return doubleAnim;
        }

        internal static DoubleAnimation CreateTo(double seconds, Action onCompleted = null)
        {
            DoubleAnimation doubleAnim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(seconds));

            if (onCompleted != null)
            {
                doubleAnim.Completed += delegate { onCompleted(); };
            }

            Timeline.SetDesiredFrameRate(doubleAnim, 120);
            return doubleAnim;
        }
    }
}
