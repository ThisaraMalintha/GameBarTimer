using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace GameBarTimer.Components
{
    public static class Timer
    {
        private static readonly string TIMER_DURATION_SETTING_KEY = "timerDuration";
        private static readonly string TIMER_SUSPEND_TIMESTAMP_SETTINGS_KEY = "timerSuspendTimestamp";

        private static DispatcherTimer _dispatcherTimer;
        private static int _durationInSeconds;
        private static int _remainingHours;
        private static int _remainingMinutes;
        private static int _remainingSeconds;

        public static event Action<int, int, int> OnSecondElapse;
        public static event Action OnTimerStop;
        public static event Action OnTimerFinish;

        static Timer()
        {
            Application.Current.Suspending += App_Suspending;
            Application.Current.Resuming += App_Resuming;
        }

        private static void App_Resuming(object sender, object e)
        {
            throw new NotImplementedException();
        }

        private static void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[TIMER_DURATION_SETTING_KEY] = _durationInSeconds;
            ApplicationData.Current.LocalSettings.Values[TIMER_SUSPEND_TIMESTAMP_SETTINGS_KEY] = DateTime.Now.Ticks;
        }

        public static void Start(int hours, int minutes)
        {
            CreateDispatcherTimerIfNotExists();

            _durationInSeconds = hours * 3600 + minutes * 60;
            _remainingHours = hours;
            _remainingMinutes = minutes;
            _remainingSeconds = 59;

            _dispatcherTimer.Start();
        }

        public static void Stop()
        {
            _dispatcherTimer.Stop();
            Application.Current.Suspending -= App_Suspending;
            Application.Current.Resuming -= App_Resuming;
        }

        private static void CreateDispatcherTimerIfNotExists()
        {
            if (_dispatcherTimer == null)
            {
                _dispatcherTimer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                _dispatcherTimer.Tick += DispatcherTimer_Tick;
            }
        }

        private static void DispatcherTimer_Tick(object sender, object e)
        {
            _remainingSeconds = Math.Max(_remainingSeconds - 1, 0);

            if (_remainingSeconds == 0 && _remainingMinutes == 0 && _remainingHours == 0)
            {
                FinishTimer();
                return;
            }

            if (_remainingSeconds == 0)
            {
                _remainingMinutes = Math.Max(_remainingMinutes - 1, 0);

                if (_remainingMinutes == 0)
                {
                    _remainingHours = Math.Max(_remainingHours - 1, 0);
                    _remainingMinutes = 59;
                }

                _remainingSeconds = 59;
            }

            OnSecondElapse.Invoke(_remainingHours, _remainingMinutes, _remainingSeconds);
        }

        private static void FinishTimer()
        {
            _dispatcherTimer.Stop();
            OnTimerFinish.Invoke();
        }
    }
}
