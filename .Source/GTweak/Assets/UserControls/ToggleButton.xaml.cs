using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GTweak.Animations;

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
                tbtn.UpdateToggleState((bool)e.NewValue, !tbtn._isUserAction);
            }
        }

        private const double _leftPositionX = -12.0, _rightPositionX = 12.0;
        private bool _isUserAction = false;

        private readonly SolidColorBrush brushOffColor = new SolidColorBrush(), brushOnColor = new SolidColorBrush(),
            borderOffColor = new SolidColorBrush(), borderOnColor = new SolidColorBrush();

        private Color _dotColorOn = (Color)Application.Current.Resources["Color_ToggleDot_On"], _dotColorOff = (Color)Application.Current.Resources["Color_ToggleDot_Off"];

        public ToggleButton()
        {
            InitializeComponent();

            brushOnColor.Color = (Color)Application.Current.Resources["Color_ToggleBG_On"];
            brushOnColor.Freeze();

            brushOffColor.Color = Colors.Transparent;
            brushOffColor.Freeze();

            borderOnColor.Color = (Color)Application.Current.Resources["Color_ToggleBG_On"];
            borderOnColor.Freeze();
            borderOffColor.Color = (Color)Application.Current.Resources["Color_ToggleBorder_Off"];
            borderOffColor.Freeze();

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
            if (IsEnabled)
            {
                _isUserAction = true;
                State = !State;
                _isUserAction = false;
                RaiseEvent(new RoutedEventArgs(ChangedStateEvent));
            }
        }

        private void UpdateToggleState(bool State, bool skipAnimation = false)
        {
            if (State)
            {
                AnimateToggle(_rightPositionX, brushOffColor, brushOnColor, borderOffColor, borderOnColor, _dotColorOn, skipAnimation, "TextToggle");
            }
            else
            {
                AnimateToggle(_leftPositionX, brushOnColor, brushOffColor, borderOnColor, borderOffColor, _dotColorOff, skipAnimation, "TextInactivity");
            }
        }

        private void AnimateToggle(double targetX, Brush fromBrush, Brush toBrush, Brush fromBorder, Brush toBorder, Color dotColor, bool skipAnimation, string textStyle)
        {
            if (skipAnimation)
            {
                DotTranslate?.BeginAnimation(TranslateTransform.XProperty, null);
                Back?.BeginAnimation(Shape.FillProperty, null);
                Back?.BeginAnimation(Shape.StrokeProperty, null);

                if (DotTranslate != null)
                {
                    DotTranslate.X = targetX;
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
                DotTranslate?.BeginAnimation(TranslateTransform.XProperty, AnimationFactory.CreateIn(DotTranslate.X, targetX, 0.2));

                if (Back != null)
                {
                    BrushAnimation brushAnimation = new BrushAnimation
                    {
                        From = fromBrush,
                        To = toBrush,
                        Duration = TimeSpan.FromMilliseconds(100)
                    };
                    Timeline.SetDesiredFrameRate(brushAnimation, 120);
                    Back.BeginAnimation(Shape.FillProperty, brushAnimation);

                    BrushAnimation borderAnimation = new BrushAnimation
                    {
                        From = fromBorder,
                        To = toBorder,
                        Duration = TimeSpan.FromMilliseconds(100)
                    };
                    Timeline.SetDesiredFrameRate(borderAnimation, 120);
                    Back.BeginAnimation(Shape.StrokeProperty, borderAnimation);
                }

                if (Dot?.Fill is SolidColorBrush solidColorBrush)
                {
                    ColorAnimation dotColorAnimation = new ColorAnimation
                    {
                        To = dotColor,
                        Duration = TimeSpan.FromMilliseconds(100),
                        EasingFunction = new QuadraticEase()
                    };
                    Timeline.SetDesiredFrameRate(dotColorAnimation, 120);
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
                DotScale.BeginAnimation(ScaleTransform.ScaleXProperty, AnimationFactory.CreateIn(DotScale.ScaleX, 1.1, 0.15, useCubicEase: true));
                DotScale.BeginAnimation(ScaleTransform.ScaleYProperty, AnimationFactory.CreateIn(DotScale.ScaleY, 1.1, 0.15, useCubicEase: true));
            }
        }

        private void Toggle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsEnabled && DotScale != null)
            {
                DotScale.BeginAnimation(ScaleTransform.ScaleXProperty, AnimationFactory.CreateIn(DotScale.ScaleX, 1, 0.15, useCubicEase: true));
                DotScale.BeginAnimation(ScaleTransform.ScaleYProperty, AnimationFactory.CreateIn(DotScale.ScaleY, 1, 0.15, useCubicEase: true));
            }
        }
    }
}