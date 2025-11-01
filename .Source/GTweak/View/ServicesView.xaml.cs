﻿using GTweak.Assets.UserControl;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GTweak.View
{
    public partial class ServicesView : UserControl
    {
        private readonly ServicesTweaks _svcTweaks = new ServicesTweaks();

        public ServicesView()
        {
            InitializeComponent();
        }

        private void Tweak_MouseEnter(object sender, MouseEventArgs e)
        {
            string description = ((ToggleButton)sender).Description?.ToString() ?? string.Empty;

            if (DescBlock.Text != description)
                DescBlock.Text = description;
        }

        private void Tweak_MouseLeave(object sender, MouseEventArgs e)
        {
            if (DescBlock.Text != DescBlock.DefaultText)
                DescBlock.Text = DescBlock.DefaultText;
        }

        private void TglButton_ChangedState(object sender, RoutedEventArgs e)
        {
            _svcTweaks.ApplyTweaks(((ToggleButton)sender).Name, ((ToggleButton)sender).State);

            NotificationManager.Show().WithDelay(300).Restart();

            Parallel.Invoke(async delegate { await Task.Delay(((ToggleButton)sender).Name.Contains("15") ? 2000 : 1000); _svcTweaks.AnalyzeAndUpdate(); });
        }
    }
}
