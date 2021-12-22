using GameBarTimer.Components;
using Microsoft.Gaming.XboxGameBar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GameBarTimer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private static bool _isTimerDetached = false;

        private TimerState _timerState = TimerState.Stopped;
        public TimerState TimerState
        {
            get { return _timerState; }
            set
            {
                _timerState = value;
                RaiseNotifyPropertyChanged();
                RaiseNotifyPropertyChanged(nameof(ShowTimerCountdown));
                RaiseNotifyPropertyChanged(nameof(ShowTimerInput));
                RaiseNotifyPropertyChanged(nameof(IsTimerRunning));
                RaiseNotifyPropertyChanged(nameof(IsTimerPaused));
                RaiseNotifyPropertyChanged(nameof(IsTimerStopped));
                RaiseNotifyPropertyChanged(nameof(IsTimerFinished));
            }
        }

        private string _countdownHours;
        public string CountdownHours
        {
            get { return _countdownHours; }
            set
            {
                _countdownHours = value;
                RaiseNotifyPropertyChanged();
            }
        }

        private string _countdownMinutes;
        public string CountdownMinutes
        {
            get { return _countdownMinutes; }
            set
            {
                _countdownMinutes = value;
                RaiseNotifyPropertyChanged();
            }
        }

        private string _countdownSeconds;
        public string CountdownSeconds
        {
            get { return _countdownSeconds; }
            set
            {
                _countdownSeconds = value;
                RaiseNotifyPropertyChanged();
            }
        }

        public bool ShowTimerCountdown => _timerState != TimerState.Stopped;
        public bool ShowTimerInput => _timerState == TimerState.Stopped;
        public bool IsTimerRunning => _timerState == TimerState.Running;
        public bool IsTimerPaused => _timerState == TimerState.Paused;
        public bool IsTimerStopped => _timerState == TimerState.Stopped;
        public bool IsTimerFinished => _timerState == TimerState.Finished;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this;
        }

        private void Timer_OnStateChanged(TimerState state)
        {
            TimerState = state;

            switch (state)
            {
                case TimerState.Running:
                    break;
                case TimerState.Paused:
                    break;
                case TimerState.Stopped:
                    break;
                case TimerState.Finished:
                    {
                        SetCountdownOutput(0, 0, 0);
                        break;
                    }
                default:
                    break;
            }
        }

        private void App_Resuming(object sender, object e)
        {
            AttachTimerSecondElapsedHandler();
        }

        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            DetachTimerSecondElapsedHandler();
        }

        private void Timer_OnSecondElapse(long remainingSeconds)
        {
            var remainingHours = remainingSeconds / 3600;
            var remainingMinutes = ((remainingSeconds - (remainingHours * 60)) / 60) % 60;
            remainingSeconds = remainingSeconds % 60;

            SetCountdownOutput(remainingHours, remainingMinutes, remainingSeconds);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                var widget = e.Parameter as XboxGameBarWidget;
                if (widget != null)
                {
                    widget.VisibleChanged += Widget_VisibleChanged;
                }
            }

            Application.Current.Suspending += App_Suspending;
            Application.Current.Resuming += App_Resuming;

            Timer.OnStateChanged += Timer_OnStateChanged;
            AttachTimerSecondElapsedHandler();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Application.Current.Suspending -= App_Suspending;
            Application.Current.Resuming -= App_Resuming;
        }

        private void Widget_VisibleChanged(XboxGameBarWidget sender, object args)
        {
            if (sender.Visible && _isTimerDetached)
            {
                AttachTimerSecondElapsedHandler();
            }
            else
            {
                DetachTimerSecondElapsedHandler();
            }
        }

        public void StartTimerBtn_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(HourInputTextBox.Text, out int hours);
            int.TryParse(MinuteInputTextBox.Text, out int minutes);

            if (hours == 0 && minutes == 0)
            {
                return;
            }

            if (_isTimerDetached)
            {
                AttachTimerSecondElapsedHandler();
            }

            Timer.OnStateChanged += Timer_OnStateChanged;

            SetCountdownOutput(hours, minutes, 0);

            Timer.Start(hours, minutes);
        }

        private void RaiseNotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void StopTimerBtn_Click(object sender, RoutedEventArgs e)
        {
            Timer.Stop();

            Timer.OnStateChanged -= Timer_OnStateChanged;
            DetachTimerSecondElapsedHandler();
        }

        private void PauseTimerBtn_Click(object sender, RoutedEventArgs e)
        {
            Timer.Pause();
            DetachTimerSecondElapsedHandler();
        }

        private void ResumeTimerBtn_Click(object sender, RoutedEventArgs e)
        {
            AttachTimerSecondElapsedHandler();
            Timer.Resume();
        }

        private void ResetTimerBtn_Click(object sender, RoutedEventArgs e)
        {
            TimerState = TimerState.Stopped;
        }

        private void AttachTimerSecondElapsedHandler()
        {
            Timer.OnSecondElapse += Timer_OnSecondElapse;
            _isTimerDetached = false;
        }

        private void DetachTimerSecondElapsedHandler()
        {
            Timer.OnSecondElapse -= Timer_OnSecondElapse;
            _isTimerDetached = true;
        }

        private void SetCountdownOutput(long remainingHours, long remainingMinutes, long remainingSeconds)
        {
            CountdownHours = remainingHours.ToString().PadLeft(0, '0');
            CountdownMinutes = remainingMinutes.ToString().PadLeft(2, '0');
            CountdownSeconds = remainingSeconds.ToString().PadLeft(2, '0');
        }
    }
}
