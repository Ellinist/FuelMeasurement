using System.Windows;
using System.Windows.Controls;
using FuelMeasurement.Common.Enums;

namespace FuelMeasurement.Client.UIModule.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ComputeTypeControl.xaml
    /// </summary>
    public partial class ComputeTypeControl : UserControl
    {
        public static ComputeTypeControl Instance { get; private set; }

        public ComputeTypeControl()
        {
            InitializeComponent();
            
            Instance = this;
        }

        public static readonly DependencyProperty CurrentTabProperty = DependencyProperty.Register("CurrentTab", typeof(int), typeof(ComputeTypeControl),
                                                                       new PropertyMetadata(OnCurrentTabPropertyChanged));

        private static void OnCurrentTabPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs param)
        {
            switch((FuelTankTabControlStates)param.NewValue)
            {
                case FuelTankTabControlStates.ErrorsTab:
                    ComputeTypeControl.Instance.UsualErrorsButton.IsEnabled = true;
                    ComputeTypeControl.Instance.UsualErrorsInButton.IsEnabled = true;
                    ComputeTypeControl.Instance.UsualErrorsOutButton.IsEnabled = true;
                    ComputeTypeControl.Instance.MirrorErrorsCommonInButton.IsEnabled = true;
                    ComputeTypeControl.Instance.MirrorErrorsOutButton.IsEnabled = true;
                    ComputeTypeControl.Instance.ReferencedErrorsButton.IsEnabled = true;
                    ComputeTypeControl.Instance.ReferencedErrorsInButton.IsEnabled = true;
                    ComputeTypeControl.Instance.ReferencedErrorsOutButton.IsEnabled = true;
                    break;
                case FuelTankTabControlStates.CalibrationTab:
                    ComputeTypeControl.Instance.UsualErrorsButton.IsEnabled = false;
                    ComputeTypeControl.Instance.UsualErrorsInButton.IsEnabled = false;
                    ComputeTypeControl.Instance.UsualErrorsOutButton.IsEnabled = false;
                    ComputeTypeControl.Instance.MirrorErrorsCommonInButton.IsEnabled = false;
                    ComputeTypeControl.Instance.MirrorErrorsOutButton.IsEnabled = false;
                    ComputeTypeControl.Instance.ReferencedErrorsButton.IsEnabled = false;
                    ComputeTypeControl.Instance.ReferencedErrorsInButton.IsEnabled = false;
                    ComputeTypeControl.Instance.ReferencedErrorsOutButton.IsEnabled = false;
                    break;
                case FuelTankTabControlStates.CapacityTab:
                    ComputeTypeControl.Instance.UsualErrorsButton.IsEnabled = true;
                    ComputeTypeControl.Instance.UsualErrorsInButton.IsEnabled = true;
                    ComputeTypeControl.Instance.UsualErrorsOutButton.IsEnabled = true;
                    ComputeTypeControl.Instance.MirrorErrorsCommonInButton.IsEnabled = false;
                    ComputeTypeControl.Instance.MirrorErrorsOutButton.IsEnabled = false;
                    ComputeTypeControl.Instance.ReferencedErrorsButton.IsEnabled = false;
                    ComputeTypeControl.Instance.ReferencedErrorsInButton.IsEnabled = false;
                    ComputeTypeControl.Instance.ReferencedErrorsOutButton.IsEnabled = false;
                    break;
            }
        }

        public int CurrentTab
        {
            get
            {
                return (int)GetValue(CurrentTabProperty);
            }
            set
            {
                SetValue(CurrentTabProperty, value);
            }
        }

        public static readonly DependencyProperty ComputeTypeProperty = DependencyProperty.Register("ComputeType", typeof(ComputeTypeEnum), typeof(ComputeTypeControl));

        public ComputeTypeEnum ComputeType
        {
            get { return (ComputeTypeEnum)GetValue(ComputeTypeProperty); }
            set { SetValue(ComputeTypeProperty, value); }
        }
    }
}