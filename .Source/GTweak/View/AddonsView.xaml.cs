using System;
using System.Windows.Controls;
using GTweak.Core.ViewModel;
using GTweak.Utilities.Managers;

namespace GTweak.View
{
    public partial class AddonsView : UserControl
    {
        private TimerControlManager _timer = default;
        public AddonsView()
        {
            InitializeComponent();

            Unloaded += delegate { _timer.Stop(); };
            Loaded += delegate
            {
                _timer = new TimerControlManager(TimeSpan.Zero, TimerControlManager.TimerMode.CountUp, async time =>
                {
                    if ((int)time.TotalSeconds % 5 == 0 && DataContext is AddonsViewModel viewModel)
                    {
                        viewModel.UpdateCommand.Execute(null);
                    }
                });

                _timer.Start();
            };
        }
    }
}