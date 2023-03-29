using FuelMeasurement.Tools.ReportModule.ViewModels;
using MahApps.Metro.Controls;

namespace FuelMeasurement.Tools.ReportModule.Views
{
    /// <summary>
    /// Логика взаимодействия для ReportView.xaml
    /// </summary>
    public partial class ReportView : MetroWindow
    {
        public ReportView(ReportViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
