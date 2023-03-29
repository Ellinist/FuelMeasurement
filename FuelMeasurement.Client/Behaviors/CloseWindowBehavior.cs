using FuelMeasurement.Client.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows;

namespace FuelMeasurement.Client.Behaviors
{
    public class CloseWindowBehavior : Behavior<Window>
    {
        public bool CloseTrigger
        {
            get => (bool)GetValue(CloseTriggerProperty);
            set => SetValue(CloseTriggerProperty, value);
        }

        public static readonly DependencyProperty CloseTriggerProperty
            = DependencyProperty.Register(
                nameof(CloseTrigger),
                typeof(bool),
                typeof(CloseWindowBehavior),
                new PropertyMetadata(false, OnCloseTriggerChanged)
                );

        private static void OnCloseTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CloseWindowBehavior behavior)
                behavior.OnCloseTriggerChanged();
        }

        private void OnCloseTriggerChanged()
        {
            if (CloseTrigger)
            {
                AssociatedObject.Close();
                if (AssociatedObject.DataContext is SplashViewModel splashViewModel)
                {
                    splashViewModel.InvokeShutdown();
                }
            }
        }
    }
}
