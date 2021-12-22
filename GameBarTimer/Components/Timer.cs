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
        private static readonly string TIMER_SUSPEND_TIMESTAMP_SETTING_KEY = "timerSuspendTimestamp";

        private static DispatcherTimer _dispatcherTimer;

        private static int _durationInSeconds;
        private static int _remainingSeconds;

        public static event Action<long> OnSecondElapse;
        public static event Action<TimerState> OnStateChanged;

        static Timer()
        {
            AttachApplicationEventHandlers();
        }

        private static void App_Resuming(object sender, object e)
        {
            if (_durationInSeconds == default)
            {
                var isTimerDurationExists = ApplicationData.Current.LocalSettings.Values.TryGetValue(TIMER_DURATION_SETTING_KEY, out var timerDurationInSeconds);
                if (!isTimerDurationExists)
                {
                    Stop();
                    return;
                }

                _durationInSeconds = Convert.ToInt32(timerDurationInSeconds);
            }

            var isSuspendTimestampExists = ApplicationData.Current.LocalSettings.Values.TryGetValue(TIMER_SUSPEND_TIMESTAMP_SETTING_KEY, out var suspendTimestampInSeconds);
            if (!isSuspendTimestampExists)
            {
                Stop();
                return;
            }

            var suspendDateTime = DateTimeOffset.FromUnixTimeSeconds((long)suspendTimestampInSeconds);
            var elapsedSecondsCount = DateTimeOffset.Now.Subtract(suspendDateTime).TotalSeconds;

            _remainingSeconds = _durationInSeconds - (int)elapsedSecondsCount;

            var remainingHours = _remainingSeconds / 3600;
            var remainingMinutes = (_remainingSeconds - (remainingHours * 60)) / 60;
            var remainingSeconds = _remainingSeconds % 60;

            Start(remainingHours, remainingMinutes, remainingSeconds);
        }

        private static void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            SaveTimerState();
        }

        private static void SaveTimerState()
        {
            ApplicationData.Current.LocalSettings.Values[TIMER_DURATION_SETTING_KEY] = _durationInSeconds;
            ApplicationData.Current.LocalSettings.Values[TIMER_SUSPEND_TIMESTAMP_SETTING_KEY] = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public static void Start(int hours, int minutes, int seconds = 0)
        {
            if (hours == 0 && minutes == 0)
            {
                throw new InvalidOperationException("Duration should be at least a minute");
            }

            CreateDispatcherTimerIfNotExists();
            ClearSavedTimerState();

            _durationInSeconds = hours * 3600 + minutes * 60;
            _remainingSeconds = _durationInSeconds;

            _dispatcherTimer.Start();

            OnStateChanged?.Invoke(TimerState.Running);
        }

        public static void Stop()
        {
            _dispatcherTimer.Stop();

            Application.Current.Suspending -= App_Suspending;
            Application.Current.Resuming -= App_Resuming;

            ClearSavedTimerState();
            DettachApplicationEventHandlers();

            OnStateChanged?.Invoke(TimerState.Stopped);
        }

        public static void Pause()
        {
            _dispatcherTimer.Stop();
            SaveTimerState();

            OnStateChanged?.Invoke(TimerState.Paused);
        }

        public static void Resume()
        {
            _dispatcherTimer.Start();

            OnStateChanged?.Invoke(TimerState.Running);
        }

        private static void ClearSavedTimerState()
        {
            ApplicationData.Current.LocalSettings.Values.Remove(TIMER_DURATION_SETTING_KEY);
            ApplicationData.Current.LocalSettings.Values.Remove(TIMER_SUSPEND_TIMESTAMP_SETTING_KEY);
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
            _remainingSeconds -= 1;
            if(_remainingSeconds == 0)
            {
                FinishTimer();
                return;
            }

            OnSecondElapse?.Invoke(_remainingSeconds);
        }

        private static void FinishTimer()
        {
            _dispatcherTimer.Stop();
            OnStateChanged?.Invoke(TimerState.Finished);
        }

        private static void AttachApplicationEventHandlers()
        {
            Application.Current.Suspending += App_Suspending;
            Application.Current.Resuming += App_Resuming;
        }
        
        private static void DettachApplicationEventHandlers()
        {
            Application.Current.Suspending -= App_Suspending;
            Application.Current.Resuming -= App_Resuming;
        }
    }
}
