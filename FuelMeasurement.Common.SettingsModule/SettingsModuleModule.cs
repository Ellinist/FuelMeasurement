using FuelMeasurement.Common.SettingsModule.ViewModels;
using FuelMeasurement.Common.SettingsModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows.Controls;

namespace FuelMeasurement.Common.SettingsModule
{
    [Module(ModuleName = "Модуль настроек программы")]
    public class SettingsModuleModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("AppSettingsView", typeof(UserControl));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ViewModelLocationProvider.Register<UserControl, AppSettingsViewModel>();
            containerRegistry.RegisterForNavigation<AppSettingsView>();
        }
    }
}