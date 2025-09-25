using GTweak.Utilities.Animation;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace GTweak.Assets.UserControl
{
    public partial class ToggleButton
    {
        /// <summary>
        /// Custom Event - Processing keypresses only for the button and not for the text
        /// </summary>
        internal event EventHandler ChangedState;

        private readonly LinearGradientBrush brushOffColor = new LinearGradientBrush(), brushOnColor = new LinearGradientBrush();
        private bool _state = false;
        private readonly Thickness _leftPosition = new Thickness(4, 0, 0, 0), _rightPosition = new Thickness(42, 0, 0, 0);

        /// <summary>
        /// Changes the state of a Toggle Button with animation
        /// </summary>
        internal bool State { get => _state; set { _state = value; UpdateToggleState(_state); } }

        /// <summary>
        /// Changes the state of a ToggleButton without animation
        /// </summary>
        internal bool StateNA
        {
            get => (bool)GetValue(StateNAProperty);
            set => SetValue(StateNAProperty, value);
        }

        /// <summary>
        /// Dependency property used for binding without triggering animations
        /// </summary>
        internal static readonly DependencyProperty StateNAProperty = DependencyProperty.Register(nameof(StateNA), typeof(bool), typeof(ToggleButton), new PropertyMetadata(false, (d, e) =>
        {
            if (d is ToggleButton tbtn)
            {
                bool newValue = (bool)e.NewValue;
                tbtn._state = newValue;
                tbtn.UpdateToggleState(newValue, true);
            }
        }));

        /// <summary>
        /// Changes the text for ToggleButton. Accepts: Dynamic and Static Resource, just a string.
        /// </summary>
        internal object TextResource
        {
            get => (string)GetValue(TextProperty);
            set
            {
                switch (value)
                {
                    case DynamicResourceExtension dynamicResource:
                        ToggleText.SetResourceReference(TextBlock.TextProperty, new DynamicResourceExtensionConverter().ConvertToString(dynamicResource.ResourceKey));
                        break;
                    case StaticResourceExtension staticResource:
                        ToggleText.SetResourceReference(TextBlock.TextProperty, staticResource.ResourceKey);
                        break;
                    case string text:
                        ToggleText.Text = text;
                        break;
                }
            }
        }

        private static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(TextResource), typeof(string), typeof(ToggleButton), new PropertyMetadata("", (s, e) => ((ToggleButton)s).ToggleText.Text = (string)e.NewValue));

        public ToggleButton()
        {
            InitializeComponent();

            brushOnColor.GradientStops.Add(new GradientStop((Color)FindResource("Color_ToggleButton_ON_1"), 0.2));
            brushOnColor.GradientStops.Add(new GradientStop((Color)FindResource("Color_ToggleButton_ON_2"), 0.9));
            brushOnColor.StartPoint = new Point(0, 0);
            brushOnColor.EndPoint = new Point(0, 1);

            brushOffColor.GradientStops.Add(new GradientStop((Color)FindResource("Color_ToggleButton_OFF_1"), 1.0));
            brushOffColor.GradientStops.Add(new GradientStop((Color)FindResource("Color_ToggleButton_OFF_2"), 1.0));

            UpdateToggleState(_state, true);
        }

        private void Toggle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() != typeof(TextBlock))
            {
                ChangedState?.Invoke(this, EventArgs.Empty);
                State = !_state;
            }
        }

        private void UpdateToggleState(bool isState, bool isSkipAnimation = false)
        {
            if (isState && Dot.Margin != _rightPosition)
                AnimateToggle(_rightPosition, brushOffColor, brushOnColor, "Text", isSkipAnimation);
            else if (!isState && Dot.Margin != _leftPosition)
                AnimateToggle(_leftPosition, brushOnColor, brushOffColor, "Text_In", isSkipAnimation);
        }

        private void AnimateToggle(Thickness targetPosition, Brush fromBrush, Brush toBrush, string textStyle, bool skipAnimation)
        {
            Dispatcher.Invoke(() =>
            {
                ThicknessAnimationUsingKeyFrames marginAnimation = new ThicknessAnimationUsingKeyFrames();
                marginAnimation.KeyFrames.Add(new EasingThicknessKeyFrame(targetPosition.Equals(_rightPosition) ? _leftPosition : _rightPosition, TimeSpan.Zero)
                {
                    EasingFunction = new QuadraticEase()
                });
                marginAnimation.KeyFrames.Add(new EasingThicknessKeyFrame(targetPosition, KeyTime.FromTimeSpan(skipAnimation ? TimeSpan.Zero : TimeSpan.FromMilliseconds(150)))
                {
                    EasingFunction = new QuadraticEase()
                });
                Timeline.SetDesiredFrameRate(marginAnimation, 240);
                Dot.BeginAnimation(MarginProperty, marginAnimation);

                BrushAnimation brushAnimation = new BrushAnimation
                {
                    From = fromBrush,
                    To = toBrush,
                    Duration = skipAnimation ? TimeSpan.Zero : TimeSpan.FromMilliseconds(150)
                };
                Timeline.SetDesiredFrameRate(brushAnimation, 240);
                Back.BeginAnimation(Shape.FillProperty, brushAnimation);

                ToggleText.Style = (Style)FindResource(textStyle);
            });
        }
    }
}
