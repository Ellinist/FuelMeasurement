using FuelMeasurement.Tools.ReportModule.ViewModels;
using System.Windows.Controls;

namespace FuelMeasurement.Tools.ReportModule.Views
{
    /// <summary>
    /// Логика взаимодействия для ReportRegionView.xaml
    /// </summary>
    public partial class ReportRegionView : UserControl
    {
        public ReportRegionView(ReportViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}