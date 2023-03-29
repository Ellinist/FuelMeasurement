using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FuelMeasurement.Client.Tools.Timer
{
    public class WPFTimer : TextBlock
    {
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.
            Register("Interval", 
            typeof(TimeSpan), 
            typeof(WPFTimer), 
            new PropertyMetadata(TimeSpan.FromSeconds(1), IntervalChangedCallback));

        public static readonly DependencyProperty IsRunningProperty = DependencyProperty.
            Register("IsRunning", 
            typeof(bool), 
            typeof(WPFTimer), 
            new PropertyMetadata(false, IsRunningChangedCallback));

        private static void IntervalChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WPFTimer wpfTimer = (WPFTimer)d;
            wpfTimer.timer.Interval = (TimeSpan)e.NewValue;
        }

        private static void IsRunningChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WPFTimer wpfTimer = (WPFTimer)d;
            wpfTimer.timer.IsEnabled = (bool)e.NewValue;
        }

        private readonly DispatcherTimer timer;

        public TimeSpan Interval
        {
            get
            {
                return (TimeSpan)this.GetValue(IntervalProperty);
            }
            set
            {
                this.SetValue(IntervalProperty, value);
            }
        }

        public bool IsRunning
        {
            get
            {
                return (bool)this.GetValue(IsRunningProperty);
            }
            set
            {
                this.SetValue(IsRunningProperty, value);
            }
        }

        public WPFTimer()
        {
            this.timer = new DispatcherTimer(this.Interval, DispatcherPriority.Normal, this.Timer_Tick, this.Dispatcher);
            this.timer.IsEnabled = false;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.SetValue(TextProperty, DateTime.Now.ToString("HH:mm"));
            this.SetValue(ToolTipProperty, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
        }
    }
}
