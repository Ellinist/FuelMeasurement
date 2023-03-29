using FuelMeasurement.Common.Enums;
using FuelMeasurement.Common.SettingsModule.Interfaces;
using FuelMeasurement.Common.SettingsModule.Models;
using System.ComponentModel;

namespace FuelMeasurement.Common.SettingsModule.Helpers
{
    public class SettingsTreeViewItemHelper : ISettingsTreeViewItemsHelper
    {
        private static readonly string _projectSettingsHeader = "Настройки проекта";
        private static readonly string _testSettingsHeader = "Для теста";
        //private static readonly string _mainWindowSettingsHeader = "Настройки главного окна";
        //private static readonly string _fuelTankWindowSettingsHeader = "Настройки окна расстановки";
        //private static readonly string _sensorsSettingsHeader = "Настройки датчиков";
        //private static readonly string _tanksSettingsHeader = "Настройки баков";
        //private static readonly string _heatMapSettingsHeader = "Настройки тепловой карты";

        public BindingList<SettingsTreeViewItem> CreateTreeViewItems()
        {
            BindingList<SettingsTreeViewItem> items = new();

            SettingsTreeViewItem airplaneConfig = new(SettingsTreeViewItemPropertyEnum.AirplaneConfigurationModel, _projectSettingsHeader);
            SettingsTreeViewItem testConfig = new(SettingsTreeViewItemPropertyEnum.TestConfigurationModel, _testSettingsHeader);
            //SettingsTreeViewItem mainWindowConfig = new(SettingsTreeViewItemPropertyEnum.MainWindowConfigurationModel, _mainWindowSettingsHeader);
            //SettingsTreeViewItem fuelTankWindowConfig = new(SettingsTreeViewItemPropertyEnum.FuelTankViewConfigurationModel, _fuelTankWindowSettingsHeader);
            //SettingsTreeViewItem sensorsConfig = new(SettingsTreeViewItemPropertyEnum.SensorsConfigurationModel, _sensorsSettingsHeader);
            //SettingsTreeViewItem fuelTanksConfig = new(SettingsTreeViewItemPropertyEnum.FuelTanksConfigurationModel, _tanksSettingsHeader);
            //SettingsTreeViewItem heatMapConfig = new(SettingsTreeViewItemPropertyEnum.HeatMapConfigurationModel, _heatMapSettingsHeader);

            items.Add(airplaneConfig);
            items.Add(testConfig);
            //items.Add(mainWindowConfig);
            //items.Add(fuelTankWindowConfig);
            //items.Add(sensorsConfig);
            //items.Add(fuelTanksConfig);
            //items.Add(heatMapConfig);

            return items;
        }
    }
}
