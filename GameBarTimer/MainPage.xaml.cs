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



        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();

            Timer.OnSecondElapse += Timer_OnSecondElapse;
        }

        private void Timer_OnSecondElapse(int remainingHours, int remainingMinutes, int remainingSeconds)
        {
            CountdownHours = remainingHours.ToString();
            CountdownMinutes = remainingMinutes.ToString().PadLeft(2, '0');
            CountdownSeconds = remainingSeconds.ToString().PadLeft(2, '0');

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                var widget = e.Parameter as XboxGameBarWidget;
                if (widget != null)
                    widget.VisibleChanged += Widget_VisibleChanged;
            }
        }

        private void Widget_VisibleChanged(XboxGameBarWidget sender, object args)
        {
            Debug.WriteLine("Visibility is " + sender.Visible);

        }

        public void StartTimerBtn_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(HourInputTextBox.Text, out int hours);
            int.TryParse(MinuteInputTextBox.Text, out int minutes);

            Timer.Start(hours, minutes);

            TimerInputPanel.Visibility = Visibility.Collapsed;
            CountDownStatusPanel.Visibility = Visibility.Visible;

            StatusTextBlock.Text = "Running";
        }

        private void RaiseNotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
