using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using GTweak.Animations;
using GTweak.Behaviors;
using GTweak.Modules.Common;

namespace GTweak.Assets.UserControls
{
    public sealed partial class DescriptionBlock
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

        internal object ContentSource
        {
            set
            {
                TargetState = null;
                string newText = DefaultText;

                switch (value)
                {
                    case ToggleButton btn:
                        TargetState = btn.State;
                        newText = btn.Description?.ToString() ?? string.Empty;
                        break;
                    case System.Windows.Controls.Primitives.ToggleButton btn:
                        newText = btn.ToolTip?.ToString() ?? string.Empty;
                        break;
                    case StackPanel panel:
                        newText = panel.ToolTip?.ToString() ?? string.Empty;
                        break;
                }

                if (_currentDefaultText != newText)
                {
                    Text = newText;
                }
            }
        }

        internal string Text
        {
            get => _currentDefaultText;
            set
            {
                string safeValue = value ?? string.Empty;
                _currentDefaultText = safeValue;

                CancellationTokenSource cts = new CancellationTokenSource();
                Interlocked.Exchange(ref _scrollCts, cts)?.Cancel();

                if (Scroller == null || FunctionDescription == null)
                {
                    return;
                }

                UpdateFlowDirection();

                Scroller.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, null);
                Scroller.ScrollToVerticalOffset(0);
                Scroller.UpdateLayout();

                StatusPanel.BeginAnimation(OpacityProperty, null);
                StatusPanel.Opacity = 0;

                TimeSpan duration = safeValue.Length <= 50 ? TimeSpan.FromMilliseconds(200) : safeValue.Length <= 200 ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(550);

                Caret.BeginAnimation(OpacityProperty, AnimationFactory.CreateIn(1.0, 0.0, 0.3, reverse: true));

                FunctionDescription.Text = string.Empty;
                TypewriterAnimation.Create(safeValue, FunctionDescription, duration);

                _ = HandleStatusAndScrollAsync(safeValue, duration, cts.Token);
            }
        }

        public DescriptionBlock()
        {
            InitializeComponent();

            Loaded += delegate
            {
                App.LanguageChanged -= OnLanguageChanged;
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
                Interlocked.Exchange(ref _scrollCts, null)?.Cancel();
            };
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            Interlocked.Exchange(ref _scrollCts, null)?.Cancel();

            if (FunctionDescription == null)
            {
                return;
            }

            UpdateFlowDirection();
            _currentDefaultText = DefaultText;
            FunctionDescription.Text = string.Empty;
            TypewriterAnimation.Create(DefaultText, FunctionDescription, TimeSpan.Zero);
        }

        private void UpdateFlowDirection()
        {
            if (FunctionDescription != null && !DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                try { FunctionDescription.FlowDirection = CultureInfo.GetCultureInfo(GlobalOptions.Language).TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight; }
                catch (CultureNotFoundException) { FunctionDescription.FlowDirection = FlowDirection.LeftToRight; }
            }
        }

        private async Task HandleStatusAndScrollAsync(string text, TimeSpan typewriterDuration, CancellationToken token)
        {
            try
            {
                await Task.Delay(typewriterDuration, token);

                await Dispatcher.InvokeAsync(() =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    Caret.BeginAnimation(OpacityProperty, null);
                    Caret.Opacity = 1;

                    if (TargetState != null && text != DefaultText)
                    {
                        StatusPanel.BeginAnimation(OpacityProperty, AnimationFactory.CreateIn(0.0, 1.0, 0.3));
                    }
                });

                await Task.Delay(1500, token);

                if (text == DefaultText)
                {
                    return;
                }

                await Dispatcher.InvokeAsync(() =>
                {
                    FunctionDescription?.UpdateLayout();
                    Scroller?.UpdateLayout();
                });

                double maxOffset = 0;
                await Dispatcher.InvokeAsync(() => maxOffset = Scroller?.ScrollableHeight ?? 0);

                if (maxOffset <= 0)
                {
                    return;
                }

                double durationSeconds = Math.Min(6.0, maxOffset / 20.0 + 0.8);

                await Dispatcher.InvokeAsync(() =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    Scroller.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0, maxOffset, TimeSpan.FromSeconds(durationSeconds)) { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } });
                });

                await Task.Delay(TimeSpan.FromSeconds(durationSeconds), token);
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }
    }
}