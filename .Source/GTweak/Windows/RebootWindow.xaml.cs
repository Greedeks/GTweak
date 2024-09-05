using GTweak.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace GTweak.Windows
{
    public partial class RebootWindow : Window
    {
        public RebootWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = 0,
                To = 100,
                EasingFunction = new QuadraticEase(),
                Duration = TimeSpan.FromSeconds(1.5)
            };
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            doubleAnim.Completed += delegate { Settings.SelfReboot(); };
            RestartProgress.BeginAnimation(ProgressBar.ValueProperty, doubleAnim);
        }
    }
}
