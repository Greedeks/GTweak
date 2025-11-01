using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GTweak.Utilities.Animation
{
    internal sealed class BrushAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(Brush);

        protected override Freezable CreateInstanceCore() => new BrushAnimation();

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(Brush), typeof(BrushAnimation));

        public Brush From
        {
            get => (Brush)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(Brush), typeof(BrushAnimation));

        public Brush To
        {
            get => (Brush)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (animationClock.CurrentProgress == null) return From ?? defaultOriginValue;

            double progress = animationClock.CurrentProgress.Value;

            var easedProgress = new QuadraticEase().Ease(progress);

            if (From is SolidColorBrush fromBrush && To is SolidColorBrush toBrush)
            {
                byte r = (byte)(fromBrush.Color.R + (toBrush.Color.R - fromBrush.Color.R) * easedProgress);
                byte g = (byte)(fromBrush.Color.G + (toBrush.Color.G - fromBrush.Color.G) * easedProgress);
                byte b = (byte)(fromBrush.Color.B + (toBrush.Color.B - fromBrush.Color.B) * easedProgress);
                byte a = (byte)(fromBrush.Color.A + (toBrush.Color.A - fromBrush.Color.A) * easedProgress);

                return new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }

            return easedProgress < 0.5 ? From ?? defaultOriginValue : To ?? defaultDestinationValue;
        }
    }
}
