using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers.Animation;
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

        private void Window_Loaded(object sender, RoutedEventArgs e) => RestartProgress.BeginAnimation(ProgressBar.ValueProperty, FadeAnimation.FadeIn(1, 1.5, () => { SettingsRepository.SelfReboot(); }));
    }
}
