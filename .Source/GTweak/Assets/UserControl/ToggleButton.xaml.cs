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

        private readonly Thickness _leftSide = new Thickness(6, 0, 0, 0), _rightSide = new Thickness(45, 0, 0, 0);
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

            AnimationToggle(_state, true);
        }

        private void Toggle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() != typeof(TextBlock))
            {
                ChangedState?.Invoke(this, EventArgs.Empty);
                State = !_state;
            }
        }

        private void AnimationToggle(bool isState, bool isSkipAnimation = false)
        {
            Dispatcher.Invoke(() =>
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
                        KeyTime = isSkipAnimation ? KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)) : KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150))
                    };

                    thicknessAnimKeyFrames.KeyFrames.Add(fromFrame);
                    thicknessAnimKeyFrames.KeyFrames.Add(toFrame);
                    Timeline.SetDesiredFrameRate(thicknessAnimKeyFrames, 240);
                    Dot.BeginAnimation(MarginProperty, thicknessAnimKeyFrames);

                    BrushAnimation brushanimation = new BrushAnimation()
                    {
                        From = brushOffColor,
                        To = brushOnColor,
                        SpeedRatio = 1,
                        Duration = isSkipAnimation ? TimeSpan.FromSeconds(0) : TimeSpan.FromMilliseconds(150)
                    };
                    Timeline.SetDesiredFrameRate(brushanimation, 240);
                    Back.BeginAnimation(Shape.FillProperty, brushanimation);
                    ToggleText.Style = (Style)FindResource("Text");
                }

                else if (!isState && Dot.Margin != _leftSide)
                {
                    EasingThicknessKeyFrame fromFrame = new EasingThicknessKeyFrame(_rightSide)
                    {
                        KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))
                    };

                    EasingThicknessKeyFrame toFrame = new EasingThicknessKeyFrame(_leftSide)
                    {
                        KeyTime = isSkipAnimation ? KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)) : KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150))
                    };

                    thicknessAnimKeyFrames.KeyFrames.Add(fromFrame);
                    thicknessAnimKeyFrames.KeyFrames.Add(toFrame);
                    Timeline.SetDesiredFrameRate(thicknessAnimKeyFrames, 240);
                    Dot.BeginAnimation(MarginProperty, thicknessAnimKeyFrames);

                    BrushAnimation brushanimation = new BrushAnimation()
                    {
                        From = brushOnColor,
                        To = brushOffColor,
                        SpeedRatio = 1,
                        Duration = isSkipAnimation ? TimeSpan.FromSeconds(0) : TimeSpan.FromMilliseconds(150)
                    };
                    Timeline.SetDesiredFrameRate(brushanimation, 240);
                    Back.BeginAnimation(Shape.FillProperty, brushanimation);
                    ToggleText.Style = (Style)FindResource("Text_In");
                }
            });
        }
    }
}
