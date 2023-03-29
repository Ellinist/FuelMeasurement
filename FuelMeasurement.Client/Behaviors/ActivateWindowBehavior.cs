using Microsoft.Xaml.Behaviors;
using System.Windows;

namespace FuelMeasurement.Client.Behaviors
{
    public class ActivateWindowBehavior : Behavior<Window>
    {
        public bool ActivateTrigger
        {
            get => (bool)GetValue(ActivateTriggerProperty);
            set => SetValue(ActivateTriggerProperty, value);
        }

        public static readonly DependencyProperty ActivateTriggerProperty
            = DependencyProperty.Register(
                nameof(ActivateTrigger),
                typeof(bool),
                typeof(ActivateWindowBehavior),
                new PropertyMetadata(false, OnActivateTriggerChanged)
                );

        private static void OnActivateTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ActivateWindowBehavior behavior)
                behavior.OnActivateTriggerChanged();
        }

        private void OnActivateTriggerChanged()
        {
            if (ActivateTrigger)
                AssociatedObject.Activate();
        }
    }
}
