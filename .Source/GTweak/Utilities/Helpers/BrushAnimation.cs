using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GTweak.Utilities.Helpers
{
    internal sealed class BrushAnimation : AnimationTimeline
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BrushAnimation();
        }

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
            Brush _fromVal = ((Brush)GetValue(FromProperty)).CloneCurrentValue();
            Brush _toVal = ((Brush)GetValue(ToProperty)).CloneCurrentValue();

            switch (animationClock.CurrentProgress)
            {
                case 0.0:
                    return _fromVal;
                case 1.0:
                    return _toVal;
            }

            _toVal.Opacity = Convert.ToDouble(animationClock.CurrentProgress);


            Border border0 = new Border();
            Border border1 = new Border();

            border0.Width = 1.0;
            border0.Height = 1.0;

            border0.Background = _fromVal;
            border1.Background = _toVal;

            border0.Visibility = Visibility.Visible;
            border1.Visibility = Visibility.Visible;
            border0.Child = border1;

            Brush VB = new VisualBrush(border0);
            return VB;

        }
    }
}
