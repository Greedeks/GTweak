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
        internal static readonly RoutedEvent ChangedStateEvent = EventManager.RegisterRoutedEvent(nameof(ChangedState), RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ToggleButton));

        internal event RoutedEventHandler ChangedState
        {
            add => AddHandler(ChangedStateEvent, value);
            remove => RemoveHandler(ChangedStateEvent, value);
        }

        private readonly SolidColorBrush brushOffColor = new SolidColorBrush();
        private readonly SolidColorBrush brushOnColor = new SolidColorBrush();
        private readonly SolidColorBrush borderOffColor = new SolidColorBrush();
        private readonly SolidColorBrush borderOnColor = new SolidColorBrush();
        private bool _state = false;

        private readonly Thickness _leftPosition = new Thickness(0, 0, 24, 0);
        private readonly Thickness _rightPosition = new Thickness(24, 0, 0, 0);

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
        /// Changes the text for ToggleButton.Accepts: Dynamic and Static Resource, just a string.
        /// </summary>
        internal object TextResource
        {
            get => (string)GetValue(TextProperty);
            set => ApplyResource(value, TextProperty, TextBlock.TextProperty, ToggleText);
        }

        private static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(TextResource), typeof(string), typeof(ToggleButton), new PropertyMetadata("", (s, e) => ((ToggleButton)s).ToggleText.Text = (string)e.NewValue));


        /// <summary>
        /// Sets the description text for the ToggleButton. Can be assigned from a DynamicResource, StaticResource, or directly as a string.
        /// </summary>
        internal object Description
        {
            get => GetValue(DescriptionProperty);
            set => ApplyResource(value, DescriptionProperty);
        }

        private static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(object), typeof(ToggleButton), new PropertyMetadata(null));

        public ToggleButton()
        {
            InitializeComponent();

            brushOnColor.Color = (Color)Application.Current.Resources["Color_ToggleBG_On"];
            brushOffColor.Color = Colors.Transparent;

            borderOnColor.Color = (Color)Application.Current.Resources["Color_ToggleBG_On"];
            borderOffColor.Color = (Color)Application.Current.Resources["Color_ToggleBorder_Off"];

            IsEnabledChanged += ToggleButton_IsEnabledChanged;

            UpdateToggleState(_state, true);
        }

        private void ApplyResource(object value, DependencyProperty dp, DependencyProperty textProperty = null, FrameworkElement target = null)
        {
            switch (value)
            {
                case DynamicResourceExtension dynamicResource:
                    if (target != null && textProperty != null)
                        target.SetResourceReference(textProperty, dynamicResource.ResourceKey);
                    else
                        SetResourceReference(dp, dynamicResource.ResourceKey);
                    break;

                case StaticResourceExtension staticResource:
                    if (target != null && textProperty != null)
                        target.SetResourceReference(textProperty, staticResource.ResourceKey);
                    else
                        SetResourceReference(dp, staticResource.ResourceKey);
                    break;

                default:
                    SetValue(dp, value);
                    if (target is TextBlock textBlock && value is string str)
                        textBlock.Text = str;
                    break;
            }
        }

        private void ToggleButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                Back.Opacity = 0.7;
                Dot.Opacity = 0.7;
                ToggleText.Opacity = 0.7;
            }
            else
            {
                Back.Opacity = 1.0;
                Dot.Opacity = 1.0;
                ToggleText.Opacity = 1.0;
            }
        }

        private void Toggle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled || sender.GetType() != typeof(TextBlock))
            {
                RaiseEvent(new RoutedEventArgs(ChangedStateEvent));
                State = !_state;
            }
        }

        private void UpdateToggleState(bool isState, bool isSkipAnimation = false)
        {
            if (isState)
                AnimateToggle(_rightPosition, brushOffColor, brushOnColor, borderOffColor, borderOnColor, (Color)Application.Current.Resources["Color_ToggleDot_On"], isSkipAnimation, "TextToggle");
            else
                AnimateToggle(_leftPosition, brushOnColor, brushOffColor, borderOnColor, borderOffColor, (Color)Application.Current.Resources["Color_ToggleDot_Off"], isSkipAnimation, "TextInactivity");
        }

        private void AnimateToggle(Thickness targetPosition, Brush fromBrush, Brush toBrush, Brush fromBorder, Brush toBorder, Color dotColor, bool skipAnimation, string textStyle)
        {
            ThicknessAnimationUsingKeyFrames marginAnimation = new ThicknessAnimationUsingKeyFrames();
            marginAnimation.KeyFrames.Add(new EasingThicknessKeyFrame(targetPosition, KeyTime.FromTimeSpan(skipAnimation ? TimeSpan.Zero : TimeSpan.FromMilliseconds(200)))
            {
                EasingFunction = new QuadraticEase()
            });
            Timeline.SetDesiredFrameRate(marginAnimation, 120);
            Dot.BeginAnimation(MarginProperty, marginAnimation);

            BrushAnimation brushAnimation = new BrushAnimation
            {
                From = fromBrush,
                To = toBrush,
                Duration = skipAnimation ? TimeSpan.Zero : TimeSpan.FromMilliseconds(100)
            };
            Timeline.SetDesiredFrameRate(brushAnimation, 120);
            Back.BeginAnimation(Shape.FillProperty, brushAnimation);

            BrushAnimation borderAnimation = new BrushAnimation
            {
                From = fromBorder,
                To = toBorder,
                Duration = skipAnimation ? TimeSpan.Zero : TimeSpan.FromMilliseconds(100)
            };
            Timeline.SetDesiredFrameRate(borderAnimation, 120);
            Back.BeginAnimation(Shape.StrokeProperty, borderAnimation);


            ColorAnimation dotColorAnimation = new ColorAnimation
            {
                To = dotColor,
                Duration = skipAnimation ? TimeSpan.Zero : TimeSpan.FromMilliseconds(100),
                EasingFunction = new QuadraticEase()
            };
            Timeline.SetDesiredFrameRate(dotColorAnimation, 120);
            ((SolidColorBrush)Dot.Fill).BeginAnimation(SolidColorBrush.ColorProperty, dotColorAnimation);

            ToggleText.Style = (Style)FindResource(textStyle);
        }

        private void Toggle_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsEnabled)
            {
                DotScale.BeginAnimation(ScaleTransform.ScaleXProperty, FactoryAnimation.CreateIn(1, 1.1, 0.15, null, false, true));
                DotScale.BeginAnimation(ScaleTransform.ScaleYProperty, FactoryAnimation.CreateIn(1, 1.1, 0.15, null, false, true));
            }
        }

        private void Toggle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsEnabled)
            {
                DotScale.BeginAnimation(ScaleTransform.ScaleXProperty, FactoryAnimation.CreateIn(1.1, 1, 0.15, null, false, true));
                DotScale.BeginAnimation(ScaleTransform.ScaleYProperty, FactoryAnimation.CreateIn(1.1, 1, 0.15, null, false, true));
            }
        }

    }
}
