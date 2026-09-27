using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using GTweak.Animations;

namespace GTweak.Assets.UserControls
{
    public partial class TaskbarPositionPicker : UserControl
    {
        public event EventHandler PositionPicked;

        private static readonly DoubleAnimation _doubleAnim = AnimationFactory.CreateIn(0.4, 1.0, 0.2, useCubicEase: true);

        internal static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(int), typeof(TaskbarPositionPicker), new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPositionChanged));

        internal int Position
        {
            get => (int)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public TaskbarPositionPicker()
        {
            InitializeComponent();
        }

        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TaskbarPositionPicker picker && picker.IsLoaded)
            {
                picker.TaskbarIndicator?.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, _doubleAnim);
                picker.TaskbarIndicator?.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, _doubleAnim);
                picker.PositionIndicator?.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, _doubleAnim);
                picker.PositionIndicator?.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, _doubleAnim);
            }
        }

        private void Sector_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.Tag is string tag && int.TryParse(tag, out var pos))
            {
                if (Position != pos)
                {
                    Position = pos;
                    PositionPicked?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}