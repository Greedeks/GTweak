using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace GTweak.Utilities.Animation
{
    internal static class FactoryAnimation
    {
        internal static DoubleAnimation CreateIn(double fromValue, double toValue, double seconds, Action onCompleted = null, bool reverse = false)
        {
            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = fromValue,
                To = toValue,
                AutoReverse = reverse,
                Duration = TimeSpan.FromSeconds(seconds),
                EasingFunction = new QuadraticEase()
            };

            if (onCompleted != null)
                doubleAnim.Completed += (s, e) => onCompleted();

            Timeline.SetDesiredFrameRate(doubleAnim, 240);
            return doubleAnim;
        }

        internal static DoubleAnimation CreateTo(double seconds, Action onCompleted = null)
        {
            DoubleAnimation doubleAnim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(seconds));

            if (onCompleted != null)
                doubleAnim.Completed += (s, e) => onCompleted();

            Timeline.SetDesiredFrameRate(doubleAnim, 240);
            return doubleAnim;
        }
    }
}
