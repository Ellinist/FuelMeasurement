using System.Windows;
using System.Windows.Controls;

namespace FuelMeasurement.Client.UIModule.UserControls
{
    /// <summary>
    /// Логика взаимодействия для CursorDataPositions.xaml
    /// </summary>
    public partial class CursorDataPositions : UserControl
    {
        public static readonly DependencyProperty CurrentVolumeProperty = DependencyProperty.Register("CurrentVolume", typeof(double), typeof(CursorDataPositions));
        public static readonly DependencyProperty NearestVolumeProperty = DependencyProperty.Register("NearestVolume", typeof(double), typeof(CursorDataPositions));
        public static readonly DependencyProperty CurrentErrorProperty = DependencyProperty.Register("CurrentError", typeof(double), typeof(CursorDataPositions));
        public static readonly DependencyProperty NearestErrorProperty = DependencyProperty.Register("NearestError", typeof(double), typeof(CursorDataPositions));

        public double CurrentVolume
        {
            get => (double)GetValue(CurrentVolumeProperty);
            set => SetValue(CurrentVolumeProperty, value);
        }

        public double NearestVolume
        {
            get => (double)GetValue(NearestVolumeProperty);
            set => SetValue(NearestVolumeProperty, value);
        }

        public double CurrentError
        {
            get => (double)GetValue(CurrentErrorProperty);
            set => SetValue(CurrentErrorProperty, value);
        }

        public double NearestError
        {
            get => (double)GetValue(NearestErrorProperty);
            set => SetValue(NearestErrorProperty, value);
        }

        public CursorDataPositions()
        {
            InitializeComponent();
        }
    }
}
