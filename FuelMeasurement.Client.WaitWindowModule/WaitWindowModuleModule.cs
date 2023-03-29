using FuelMeasurement.Client.WaitWindowModule.Interfaces;
using FuelMeasurement.Client.WaitWindowModule.Models;
using FuelMeasurement.Client.WaitWindowModule.ViewModels;
using FuelMeasurement.Client.WaitWindowModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using System;

namespace FuelMeasurement.Client.WaitWindowModule
{
    [Module(ModuleName = "Модуль ожидания")]
    public class WaitWindowModuleModule: IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterSingleton<IWaitWindowService, WaitWindowService>();
            containerRegistry.RegisterSingleton<WaitWindowViewModel>();
        }
    }
}
