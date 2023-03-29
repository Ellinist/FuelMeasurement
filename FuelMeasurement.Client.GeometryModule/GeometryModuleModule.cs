using FuelMeasurement.Client.GeometryModule.ViewModels;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;

namespace FuelMeasurement.Client.GeometryModule
{
    [Module(ModuleName = "Модуль работы с геометрией")]
    public class GeometryModuleModule : IModule
    {
        #region IModule
        public void OnInitialized(IContainerProvider containerProvider)
        {
            //var regionManager = containerProvider.Resolve<IRegionManager>();
            //regionManager.RegisterViewWithRegion(GeometryModuleRegionNames.GeometryRegion, typeof(Window));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //ViewModelLocationProvider.Register<Window, GeometryViewModel>();
            //containerRegistry.RegisterForNavigation<GeometryView>();
        }
        #endregion
    }
}
