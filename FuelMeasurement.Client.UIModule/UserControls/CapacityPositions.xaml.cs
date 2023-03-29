using System.Windows;
using System.Windows.Controls;

namespace FuelMeasurement.Client.UIModule.UserControls
{
    /// <summary>
    /// Логика взаимодействия для CapacityPositions.xaml
    /// </summary>
    public partial class CapacityPositions : UserControl
    {
        public static readonly DependencyProperty CapacityVolumeProperty = DependencyProperty.Register("CapacityVolume", typeof(double), typeof(CapacityPositions));
        public static readonly DependencyProperty NearestCapacityVolumeProperty = DependencyProperty.Register("NearestCapacityVolume", typeof(double), typeof(CapacityPositions));
        public static readonly DependencyProperty CurrentCapacityProperty = DependencyProperty.Register("CurrentCapacity", typeof(double), typeof(CapacityPositions));
        public static readonly DependencyProperty NearestCapacityProperty = DependencyProperty.Register("NearestCapacity", typeof(double), typeof(CapacityPositions));

        public double CapacityVolume
        {
            get => (double)GetValue(CapacityVolumeProperty);
            set => SetValue(CapacityVolumeProperty, value);
        }

        public double NearestCapacityVolume
        {
            get => (double)GetValue(NearestCapacityVolumeProperty);
            set => SetValue(NearestCapacityVolumeProperty, value);
        }

        public double CurrentCapacity
        {
            get => (double)GetValue(CurrentCapacityProperty);
            set => SetValue(CurrentCapacityProperty, value);
        }

        public double NearestCapacity
        {
            get => (double)GetValue(NearestCapacityProperty);
            set => SetValue(NearestCapacityProperty, value);
        }

        public CapacityPositions()
        {
            InitializeComponent();
        }
    }
}
