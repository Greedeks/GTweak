using System;
using System.Windows;
using System.Windows.Threading;

namespace GTweak.Utilities.Managers
{
    internal sealed class TimerControlManager
    {
        private readonly DispatcherTimer _timer;
        private TimeSpan _time;
        private readonly TimerMode _mode;
        private readonly Action<TimeSpan> _action;
        private readonly Action _onFinished;

        internal enum TimerMode { CountUp, CountDown }

        internal TimerControlManager(TimeSpan startTime, TimerMode mode, Action<TimeSpan> action, Action onFinished = null, TimeSpan? interval = null)
        {
            _time = startTime;
            _mode = mode;
            _action = action;
            _onFinished = onFinished;

            _timer = new DispatcherTimer(interval ?? TimeSpan.FromSeconds(1), DispatcherPriority.Normal, TimerTick, Application.Current.Dispatcher);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _action?.Invoke(_time);

            if (_mode == TimerMode.CountDown)
            {
                if (_time <= TimeSpan.Zero)
                {
                    Stop();
                    _onFinished?.Invoke();
                    return;
                }
                _time -= _timer.Interval;
            }
            else
            {
                _time += _timer.Interval;
            }
        }

        internal void Start() => _timer.Start();

        internal void Stop()
        {
            _timer.Stop();
            _timer.Tick -= TimerTick;
        }
    }
}
