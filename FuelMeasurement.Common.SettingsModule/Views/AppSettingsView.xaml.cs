using FuelMeasurement.Common.SettingsModule.Interfaces;
using FuelMeasurement.Common.SettingsModule.ViewModels;
using System.Windows.Controls;

namespace FuelMeasurement.Common.SettingsModule.Views
{
    /// <summary>
    /// Логика взаимодействия для AppSettingsView.xaml
    /// </summary>
    public partial class AppSettingsView : UserControl
    {
        public AppSettingsView(AppSettingsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
