using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Controls;

namespace GTweak.Assets.UserControl
{
    public partial class DescriptionBlock
    {
        private CancellationTokenSource _scrollCts;

        internal string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(DescriptionBlock), new UIPropertyMetadata(string.Empty));

        internal string Text
        {
            get => FunctionDescription.Text;
            set
            {
                _scrollCts?.Cancel();
                Scroller.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, null);
                Scroller.ScrollToVerticalOffset(0);
                Scroller.UpdateLayout();

                TypewriterAnimation.Create(value, FunctionDescription,
                 value.Length <= 50 ? TimeSpan.FromMilliseconds(200) : value.Length <= 200 ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(550));

                _scrollCts = new CancellationTokenSource();
                _ = StartAutoScrollAsync(value, _scrollCts.Token);
            }
        }

        public DescriptionBlock()
        {
            InitializeComponent();

            Loaded += delegate { TypewriterAnimation.Create(DefaultText, FunctionDescription, TimeSpan.FromMilliseconds(300)); };
            Unloaded += delegate { _scrollCts?.Cancel(); };

            App.LanguageChanged += delegate
            {
                _scrollCts?.Cancel();
                TypewriterAnimation.Create(DefaultText, FunctionDescription, TimeSpan.Zero);
            };
        }

        private void DescriptionBlock_Loaded(object sender, RoutedEventArgs e) => TypewriterAnimation.Create(DefaultText, FunctionDescription, TimeSpan.FromMilliseconds(300));

        private async Task StartAutoScrollAsync(string text, CancellationToken token)
        {
            await Task.Delay(2000, token);

            if (text == DefaultText || token.IsCancellationRequested)
            {
                return;
            }

            await Dispatcher.InvokeAsync(() =>
            {
                FunctionDescription.UpdateLayout();
                Scroller.UpdateLayout();
                Scroller.ScrollToVerticalOffset(0);
            });

            double maxOffset = Scroller.ScrollableHeight;
            if (maxOffset <= 0)
            {
                return;
            }

            double durationSeconds = Math.Max(2.0, maxOffset / 20);

            await Dispatcher.InvokeAsync(() => { Scroller.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, FactoryAnimation.CreateIn(0, maxOffset, durationSeconds, useCubicEase: true)); });

            try { await Task.Delay(TimeSpan.FromSeconds(durationSeconds), token); }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        private static class ScrollViewerBehavior
        {
            internal static readonly DependencyProperty VerticalOffsetProperty =
                DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVerticalOffsetChanged));

            internal static double GetVerticalOffset(ScrollViewer viewer) => (double)viewer.GetValue(VerticalOffsetProperty);

            internal static void SetVerticalOffset(ScrollViewer viewer, double value) => viewer.SetValue(VerticalOffsetProperty, value);

            private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                if (d is ScrollViewer viewer)
                {
                    viewer.ScrollToVerticalOffset((double)e.NewValue);
                }
            }
        }
    }
}