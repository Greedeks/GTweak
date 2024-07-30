using GTweak.Utilities.Helpers;
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

        private readonly Thickness _leftSide = new Thickness(-37, 0, 0, 0), _rightSide = new Thickness(0, 0, -40, 0);
        private readonly LinearGradientBrush brushOffColor = new LinearGradientBrush(), brushOnColor = new LinearGradientBrush();
        private bool _state = false;

        /// <summary>
        /// Changes the state of a Toggle Button with animation
        /// </summary>
        internal bool State { get => _state; set { _state = value; AnimationToggle(_state); } }
        /// <summary>
        /// Changes the state of a ToggleButton without animation
        /// </summary>
        internal bool StateNA { get => _state; set { _state = value; AnimationToggle(_state, true); } }
        /// <summary>
        /// Only a dimanimetic text binding
        /// </summary>
        internal DynamicResourceExtension DynamicText { set => ToggleText.SetResourceReference(TextBlock.TextProperty, new DynamicResourceExtensionConverter().ConvertToString(value.ResourceKey)); }

        internal string СhangeText
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        internal static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(СhangeText), typeof(string), typeof(ToggleButton), new PropertyMetadata("", (s, e) => ((ToggleButton)s).ToggleText.Text = (string)e.NewValue));

        public ToggleButton()
        {
            InitializeComponent();

            brushOnColor.GradientStops.Add(new GradientStop(Color.FromArgb(255, 84, 255, 159), 0.2));
            brushOnColor.GradientStops.Add(new GradientStop(Color.FromArgb(255, 36, 255, 132), 0.9));
            brushOnColor.StartPoint = new Point(0, 0);
            brushOnColor.EndPoint = new Point(0, 1);

            brushOffColor.GradientStops.Add(new GradientStop(Color.FromArgb(255, 80, 80, 80), 1.0));
            brushOffColor.GradientStops.Add(new GradientStop(Color.FromArgb(255, 105, 105, 105), 1.0));

            AnimationToggle(_state, true);
        }

        private void Toggle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType().Name != "TextBlock")
            {
                ChangedState?.Invoke(this, EventArgs.Empty);
                State = !_state;
            }
            else
                ChangedState?.Invoke(this, EventArgs.Empty);
        }

        private void AnimationToggle(bool isState, bool isSkipAnimation = false)
        {
            ThicknessAnimationUsingKeyFrames thicknessAnimKeyFrames = new ThicknessAnimationUsingKeyFrames();

            if (isState && Dot.Margin != _rightSide)
            {
                EasingThicknessKeyFrame fromFrame = new EasingThicknessKeyFrame(_leftSide)
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))
                };

                EasingThicknessKeyFrame toFrame = new EasingThicknessKeyFrame(_rightSide)
                {
                    KeyTime = isSkipAnimation ? KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)) : KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))
                };

                thicknessAnimKeyFrames.KeyFrames.Add(fromFrame);
                thicknessAnimKeyFrames.KeyFrames.Add(toFrame);
                Timeline.SetDesiredFrameRate(thicknessAnimKeyFrames, 400);
                Dot.BeginAnimation(MarginProperty, thicknessAnimKeyFrames);

                BrushAnimation brushanimation = new BrushAnimation()
                {
                    From = brushOffColor,
                    To = brushOnColor,
                    SpeedRatio = 1,
                    Duration = isSkipAnimation ? TimeSpan.FromSeconds(0) : TimeSpan.FromSeconds(0.1)
                };
                Timeline.SetDesiredFrameRate(brushanimation, 400);
                Back.BeginAnimation(Shape.FillProperty, brushanimation);
                ToggleText.Style = (Style)Application.Current.Resources["Text"];
            }

            else if (!isState && Dot.Margin != _leftSide)
            {
                EasingThicknessKeyFrame fromFrame = new EasingThicknessKeyFrame(_rightSide)
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))
                };

                EasingThicknessKeyFrame toFrame = new EasingThicknessKeyFrame(_leftSide)
                {
                    KeyTime = isSkipAnimation ? KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)) : KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))
                };

                thicknessAnimKeyFrames.KeyFrames.Add(fromFrame);
                thicknessAnimKeyFrames.KeyFrames.Add(toFrame);
                Timeline.SetDesiredFrameRate(thicknessAnimKeyFrames, 400);
                Dot.BeginAnimation(MarginProperty, thicknessAnimKeyFrames);

                BrushAnimation brushanimation = new BrushAnimation()
                {
                    From = brushOnColor,
                    To = brushOffColor,
                    SpeedRatio = 1,
                    Duration = isSkipAnimation ? TimeSpan.FromSeconds(0) : TimeSpan.FromSeconds(0.1)
                };
                Timeline.SetDesiredFrameRate(brushanimation, 400);
                Back.BeginAnimation(Shape.FillProperty, brushanimation);
                ToggleText.Style = (Style)Application.Current.Resources["Text_In"];
            }
        }
    }
}
