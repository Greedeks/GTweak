using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GTweak.Utilities.Helpers
{
    internal sealed class BrushAnimation : AnimationTimeline
    {
        protected override Freezable CreateInstanceCore() => new BrushAnimation();

        public override Type TargetPropertyType => typeof(Brush);

        static BrushAnimation()
        {
            FromProperty = DependencyProperty.Register("From", typeof(Brush),
              typeof(BrushAnimation));

            ToProperty = DependencyProperty.Register("To", typeof(Brush),
              typeof(BrushAnimation));
        }

        internal static readonly DependencyProperty FromProperty;
        internal Brush From
        {
            get => (Brush)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        internal static readonly DependencyProperty ToProperty;
        internal Brush To
        {
            get => (Brush)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public override object GetCurrentValue(object defaultOriginValue,
        object defaultDestinationValue, AnimationClock animationClock)
        {
            Brush fromVal = ((Brush)GetValue(FromProperty)).CloneCurrentValue();
            Brush toVal = ((Brush)GetValue(ToProperty)).CloneCurrentValue();

            switch (animationClock.CurrentProgress)
            {
                case 0.0:
                    return fromVal;
                case 1.0:
                    return toVal;
            }

            toVal.Opacity = Convert.ToDouble(animationClock.CurrentProgress);

            Border borderFrom = new Border();
            Border borderTo = new Border();

            borderFrom.Width = 1.0;
            borderFrom.Height = 1.0;

            borderFrom.Background = fromVal;
            borderTo.Background = toVal;

            borderFrom.Visibility = Visibility.Visible;
            borderTo.Visibility = Visibility.Visible;
            borderFrom.Child = borderTo;

            Brush vb = new VisualBrush(borderFrom);
            return vb;

        }
    }
}
