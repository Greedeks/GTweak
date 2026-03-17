using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GTweak.Utilities.Animation;

namespace GTweak.Assets.UserControls
{
    public partial class ToggleButton
    {
        /// <summary>
        /// Custom Event - Processing keypresses only for the button and not for the text
        /// </summary>
        internal static readonly RoutedEvent ChangedStateEvent =
            EventManager.RegisterRoutedEvent(nameof(ChangedState), RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ToggleButton));

        private static readonly DependencyProperty StateProperty =
           DependencyProperty.Register(nameof(State), typeof(bool), typeof(ToggleButton), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnStateChanged));

        private static readonly DependencyProperty TextProperty =
         DependencyProperty.Register(nameof(TextResource), typeof(string), typeof(ToggleButton), new PropertyMetadata("", (s, e) => { if (s is ToggleButton btn && btn.ToggleText != null) { btn.ToggleText.Text = e.NewValue as string; } }));

        private static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(object), typeof(ToggleButton), new PropertyMetadata(null));

        internal event RoutedEventHandler ChangedState
        {
            add => AddHandler(ChangedStateEvent, value);
            remove => RemoveHandler(ChangedStateEvent, value);
        }

        /// <summary>
        /// Gets or sets the current state of the ToggleButton. Supports two-way binding (MVVM) and automatically handles state transition animations.
        /// </summary>
        internal bool State
        {
            get => (bool)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        /// <summary>
        /// Changes the text for ToggleButton.Accepts: Dynamic and Static Resource, just a string.
        /// </summary>
        internal object TextResource
        {
            get => (string)GetValue(TextProperty);
            set
            {
                if (value != null)
                {
                    ApplyResource(value, TextProperty, TextBlock.TextProperty, ToggleText);
                }
            }
        }

        /// <summary>
        /// Sets the description text for the ToggleButton. Can be assigned from a DynamicResource, StaticResource, or directly as a string.
        /// </summary>
        internal object Description
        {
            get => GetValue(DescriptionProperty);
            set
            {
                if (value != null)
                {
                    ApplyResource(value, DescriptionProperty);
                }
            }
        }

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleButton tbtn)
            {
                tbtn.UpdateToggleState((bool)e.NewValue, !tbtn.IsLoaded);
            }
        }

        private readonly SolidColorBrush brushOffColor = new SolidColorBrush(), brushOnColor = new SolidColorBrush(),
            borderOffColor = new SolidColorBrush(), borderOnColor = new SolidColorBrush();

        private readonly Thickness _leftPosition = new Thickness(0, 0, 24, 0), _rightPosition = new Thickness(24, 0, 0, 0);

        public ToggleButton()
        {
            InitializeComponent();

            brushOnColor.Color = (Color)Application.Current.Resources["Color_ToggleBG_On"];
            brushOffColor.Color = Colors.Transparent;

            borderOnColor.Color = (Color)Application.Current.Resources["Color_ToggleBG_On"];
            borderOffColor.Color = (Color)Application.Current.Resources["Color_ToggleBorder_Off"];

            IsEnabledChanged += ToggleButton_IsEnabledChanged;

            UpdateToggleState(State, true);
        }

        private void ApplyResource(object value, DependencyProperty dp, DependencyProperty textProperty = null, FrameworkElement target = null)
        {
            if (value != null && (dp != null || target != null))
            {
                switch (value)
                {
                    case DynamicResourceExtension dynamicResource:
                        if (dynamicResource.ResourceKey != null)
                        {
                            if (target != null && textProperty != null)
                            {
                                target.SetResourceReference(textProperty, dynamicResource.ResourceKey);
                            }
                            else if (dp != null)
                            {
                                SetResourceReference(dp, dynamicResource.ResourceKey);
                            }
                        }
                        break;
                    case StaticResourceExtension staticResource:
                        if (staticResource.ResourceKey != null)
                        {
                            if (target != null && textProperty != null)
                            {
                                target.SetResourceReference(textProperty, staticResource.ResourceKey);
                            }
                            else if (dp != null)
                            {
                                SetResourceReference(dp, staticResource.ResourceKey);
                            }
                        }
                        break;
                    default:
                        if (dp != null)
                        {
                            SetValue(dp, value);
                        }
                        if (target is TextBlock textBlock && value is string str)
                        {
                            textBlock.Text = str;
                        }
                        break;
                }
            }
        }

        private void ToggleButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Back != null && Dot != null && ToggleText != null)
            {
                if (e.NewValue is bool newBool && !newBool)
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
        }

        private void Toggle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled || sender?.GetType() != typeof(TextBlock))
            {
                RaiseEvent(new RoutedEventArgs(ChangedStateEvent));
                State = !State;
            }
        }

        private void UpdateToggleState(bool State, bool skipAnimation = false)
        {
            if (State)
            {
                AnimateToggle(_rightPosition, brushOffColor, brushOnColor, borderOffColor, borderOnColor, (Color)Application.Current.Resources["Color_ToggleDot_On"], skipAnimation, "TextToggle");
            }
            else
            {
                AnimateToggle(_leftPosition, brushOnColor, brushOffColor, borderOnColor, borderOffColor, (Color)Application.Current.Resources["Color_ToggleDot_Off"], skipAnimation, "TextInactivity");
            }
        }

        private void AnimateToggle(Thickness targetPosition, Brush fromBrush, Brush toBrush, Brush fromBorder, Brush toBorder, Color dotColor, bool skipAnimation, string textStyle)
        {
            if (skipAnimation)
            {
                Dot?.BeginAnimation(MarginProperty, null);
                Back?.BeginAnimation(Shape.FillProperty, null);
                Back?.BeginAnimation(Shape.StrokeProperty, null);

                if (Dot != null)
                {
                    Dot.Margin = targetPosition;
                }

                if (Back != null)
                {
                    Back.Fill = toBrush;
                    Back.Stroke = toBorder;
                }

                if (Dot?.Fill is SolidColorBrush solidColorBrush)
                {
                    solidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
                    solidColorBrush.Color = dotColor;
                }
            }
            else
            {
                ThicknessAnimationUsingKeyFrames marginAnimation = new ThicknessAnimationUsingKeyFrames();
                marginAnimation.KeyFrames.Add(new EasingThicknessKeyFrame(targetPosition, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)))
                {
                    EasingFunction = new QuadraticEase()
                });
                Timeline.SetDesiredFrameRate(marginAnimation, 120);
                Dot?.BeginAnimation(MarginProperty, marginAnimation);

                BrushAnimation brushAnimation = new BrushAnimation
                {
                    From = fromBrush,
                    To = toBrush,
                    Duration = TimeSpan.FromMilliseconds(100)
                };
                Timeline.SetDesiredFrameRate(brushAnimation, 120);
                Back?.BeginAnimation(Shape.FillProperty, brushAnimation);

                BrushAnimation borderAnimation = new BrushAnimation
                {
                    From = fromBorder,
                    To = toBorder,
                    Duration = TimeSpan.FromMilliseconds(100)
                };
                Timeline.SetDesiredFrameRate(borderAnimation, 120);
                Back?.BeginAnimation(Shape.StrokeProperty, borderAnimation);

                ColorAnimation dotColorAnimation = new ColorAnimation
                {
                    To = dotColor,
                    Duration = TimeSpan.FromMilliseconds(100),
                    EasingFunction = new QuadraticEase()
                };
                Timeline.SetDesiredFrameRate(dotColorAnimation, 120);

                if (Dot?.Fill is SolidColorBrush solidColorBrush)
                {
                    solidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, dotColorAnimation);
                }
            }

            if (ToggleText != null && TryFindResource(textStyle) is Style foundStyle)
            {
                ToggleText.Style = foundStyle;
            }
        }

        private void Toggle_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsEnabled && DotScale != null)
            {
                DotScale.BeginAnimation(ScaleTransform.ScaleXProperty, FactoryAnimation.CreateIn(1, 1.1, 0.15, useCubicEase: true));
                DotScale.BeginAnimation(ScaleTransform.ScaleYProperty, FactoryAnimation.CreateIn(1, 1.1, 0.15, useCubicEase: true));
            }
        }

        private void Toggle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsEnabled && DotScale != null)
            {
                DotScale.BeginAnimation(ScaleTransform.ScaleXProperty, FactoryAnimation.CreateIn(1.1, 1, 0.15, useCubicEase: true));
                DotScale.BeginAnimation(ScaleTransform.ScaleYProperty, FactoryAnimation.CreateIn(1.1, 1, 0.15, useCubicEase: true));
            }
        }
    }
}