using FuelMeasurement.Common.SettingsModule.ViewModels.DetailedViewModels;
using System.Windows.Controls;

namespace FuelMeasurement.Common.SettingsModule.Views.DetailedViews
{
    /// <summary>
    /// Логика взаимодействия для AppConfigurationView.xaml
    /// </summary>
    public partial class AppConfigurationView : UserControl
    {
        public AppConfigurationView()
        {
            InitializeComponent();
            DataContext = new AppConfigurationViewModel();
        }
    }
}
