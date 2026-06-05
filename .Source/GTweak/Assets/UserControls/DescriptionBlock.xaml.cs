using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Behaviors;
using GTweak.Utilities.Controls;

namespace GTweak.Assets.UserControls
{
    public partial class DescriptionBlock
    {
        private CancellationTokenSource _scrollCts;
        private string _currentDefaultText = string.Empty;

        internal static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(DescriptionBlock), new UIPropertyMetadata(string.Empty));

        internal static readonly DependencyProperty TargetStateProperty =
            DependencyProperty.Register("TargetState", typeof(bool?), typeof(DescriptionBlock), new UIPropertyMetadata(null));

        internal string DefaultText
        {
            get => (string)GetValue(DefaultTextProperty);
            set => SetValue(DefaultTextProperty, value);
        }

        internal bool? TargetState
        {
            get => (bool?)GetValue(TargetStateProperty);
            set => SetValue(TargetStateProperty, value);
        }

        internal string Text
        {
            get => _currentDefaultText;
            set
            {
                string safeValue = value ?? string.Empty;
                _currentDefaultText = safeValue;

                _scrollCts?.Cancel();

                if (Scroller != null && FunctionDescription != null)
                {
                    UpdateFlowDirection();

                    Scroller.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, null);
                    Scroller.ScrollToVerticalOffset(0);
                    Scroller.UpdateLayout();

                    StatusPanel.BeginAnimation(OpacityProperty, null);
                    StatusPanel.Opacity = 0;

                    TimeSpan duration = safeValue.Length <= 50 ? TimeSpan.FromMilliseconds(200) : safeValue.Length <= 200 ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(550);

                    Caret.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(1.0, 0.0, 0.3, reverse: true));

                    FunctionDescription.Text = string.Empty;
                    TypewriterAnimation.Create(safeValue, FunctionDescription, duration);

                    _scrollCts = new CancellationTokenSource();
                    _ = HandleStatusAndScrollAsync(safeValue, duration, _scrollCts.Token);
                }
            }
        }

        public DescriptionBlock()
        {
            InitializeComponent();

            Loaded += delegate
            {
                App.LanguageChanged += OnLanguageChanged;

                if (FunctionDescription != null)
                {
                    UpdateFlowDirection();
                    _currentDefaultText = DefaultText;
                    TypewriterAnimation.Create(DefaultText, FunctionDescription, TimeSpan.FromMilliseconds(300));
                }
            };

            Unloaded += delegate
            {
                App.LanguageChanged -= OnLanguageChanged;
                _scrollCts?.Cancel();
            };
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            _scrollCts?.Cancel();
            if (FunctionDescription != null)
            {
                UpdateFlowDirection();
                _currentDefaultText = DefaultText;
                FunctionDescription.Text = string.Empty;
                TypewriterAnimation.Create(DefaultText, FunctionDescription, TimeSpan.Zero);
            }
        }

        private void UpdateFlowDirection()
        {
            if (FunctionDescription != null && !DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                try
                {
                    FunctionDescription.FlowDirection = CultureInfo.GetCultureInfo(SettingsEngine.Language).TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                }
                catch (CultureNotFoundException) { FunctionDescription.FlowDirection = FlowDirection.LeftToRight; }
            }
        }

        private async Task HandleStatusAndScrollAsync(string text, TimeSpan typewriterDuration, CancellationToken token)
        {
            await Task.Delay(typewriterDuration, token);

            if (!token.IsCancellationRequested)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    if (!token.IsCancellationRequested)
                    {
                        Caret.BeginAnimation(OpacityProperty, null);
                        Caret.Opacity = 1;

                        if (TargetState != null && text != DefaultText)
                        {
                            StatusPanel.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0.0, 1.0, 0.3));
                        }
                    }
                });

                await Task.Delay(1500, token);

                if (text != DefaultText && !token.IsCancellationRequested)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        if (FunctionDescription != null && Scroller != null)
                        {
                            FunctionDescription.UpdateLayout();
                            Scroller.UpdateLayout();
                        }
                    });

                    if (Scroller != null && !token.IsCancellationRequested)
                    {
                        double maxOffset = Scroller.ScrollableHeight;
                        if (maxOffset > 0)
                        {
                            double durationSeconds = Math.Min(6.0, (maxOffset / 20.0) + 0.8);

                            await Dispatcher.InvokeAsync(() =>
                            {
                                DoubleAnimation inertiaAnimation = new DoubleAnimation(0, maxOffset, TimeSpan.FromSeconds(durationSeconds))
                                {
                                    EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
                                };

                                Scroller?.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, inertiaAnimation);
                            });

                            try
                            {
                                await Task.Delay(TimeSpan.FromSeconds(durationSeconds), token);
                            }
                            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                        }
                    }
                }
            }
        }
    }
}