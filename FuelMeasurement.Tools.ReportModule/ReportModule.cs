using FuelMeasurement.Tools.ReportModule.ViewModels;
using FuelMeasurement.Tools.ReportModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;
using System.Windows.Controls;

namespace FuelMeasurement.Tools.ReportModule
{
    [Module(ModuleName="Модуль генерации отчетов")]
    public class ReportModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("ReportRegionView", typeof(UserControl));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ViewModelLocationProvider.Register<Window, ReportViewModel>();
            containerRegistry.RegisterForNavigation<ReportRegionView>();
        }
    }
}