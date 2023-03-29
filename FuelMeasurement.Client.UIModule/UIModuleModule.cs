using FuelMeasurement.Client.UIModule.Infrastructure;
using FuelMeasurement.Client.UIModule.Services.Implementations;
using FuelMeasurement.Client.UIModule.Services.Interfaces;
using FuelMeasurement.Client.UIModule.UserControls;
using FuelMeasurement.Client.UIModule.ViewModels;
using FuelMeasurement.Client.UIModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;

namespace FuelMeasurement.Client.UIModule
{
    [Module(ModuleName = "Модуль расстановки датчиков")]
    public class UIModuleModule : IModule
    {
        #region IModule

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(UIModuleRegionNames.ViewsRegion, typeof(Window));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ViewModelLocationProvider.Register<Viewer3DView, Viewer3DViewModel>();
            ViewModelLocationProvider.Register<Viewer3DMenuView, Viewer3DMenuViewModel>();

            containerRegistry.RegisterForNavigation<Viewer3DView>();
            containerRegistry.RegisterSingleton<IIndentService, IndentService>();
            containerRegistry.RegisterSingleton<IHeatMapService, HeatMapService>();
            containerRegistry.RegisterSingleton<IMaterialService, MaterialService>();
            containerRegistry.RegisterSingleton<IPlotterSettingsService, PlotterSettingsService>();
        }

        #endregion
    }
}
