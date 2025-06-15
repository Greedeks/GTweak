using GTweak.Utilities.Animation;
using GTweak.Utilities.Controls;
using System.Windows;
using System.Windows.Controls;

namespace GTweak.Windows
{
    public partial class RebootWindow
    {
        public RebootWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => RestartProgress.BeginAnimation(ProgressBar.ValueProperty, FadeAnimation.FadeIn(100, 1.5, () => { SettingsEngine.SelfReboot(); }));
    }
}
