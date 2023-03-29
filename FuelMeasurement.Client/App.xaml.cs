using FuelMeasurement.Client.Views;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Runtime.Loader;
using System.Reflection;

using Prism.Ioc;
using Prism.Events;
using Prism.Modularity;

using System.IO;

using NLog;
using FuelMeasurement.Data.Repositories.Repositories.Interfaces;
using FuelMeasurement.Data.Repositories.Repositories.Implementations;
using Prism.DryIoc;
using DryIoc;
using FuelMeasurement.Common.Events.SplashEvents;
using FuelMeasurement.Tools.Geometry.Interfaces;
using FuelMeasurement.Tools.Geometry.Implementations;
using FuelMeasurement.Tools.Geometry.Interfaces.TriFormat;
using FuelMeasurement.Tools.Geometry.Implementations.TriFormat;
using FuelMeasurement.Tools.Geometry.Interfaces.TxtFormat;
using FuelMeasurement.Tools.Geometry.Implementations.TxtFormat;
using FuelMeasurement.Tools.TaringModule.Interfaces;
using FuelMeasurement.Tools.TaringModule.Implementations;
using FuelMeasurement.Tools.Compute.Interfaces;
using FuelMeasurement.Tools.Compute.Implementations;
using FuelMeasurement.Client.UIModule.Services.Interfaces;
using FuelMeasurement.Client.UIModule.Services.Implementations;
using FuelMeasurement.Tools.ReportModule.Interfaces;
using FuelMeasurement.Tools.ReportModule.Implementations;
using FuelMeasurement.Tools.ReportModule.Helpers;
using FuelMeasurement.Tools.ComputeModule.Interfaces;
using FuelMeasurement.Client.WaitWindowModule.Interfaces;
using FuelMeasurement.Client.WaitWindowModule.Models;
using AutoMapper;
using FuelMeasurement.Client.Mapper;
using FuelMeasurement.Client.Services.Interfaces;
using FuelMeasurement.Client.Services.Implementations;
using FuelMeasurement.Tools.Plotter.Interfaces;
using FuelMeasurement.Tools.Plotter.Implementations;
using FuelMeasurement.Common.DialogModule.Views;
using FuelMeasurement.Common.DialogModule.ViewModels;
using FuelMeasurement.Common.DialogModule.Interfaces;
using FuelMeasurement.Common.DialogModule.Implementations;
using FuelMeasurement.Common.SettingsModule.Interfaces;
using FuelMeasurement.Common.SettingsModule.Helpers;
using FuelMeasurement.Tools.FileManager.Interfaces;
using FuelMeasurement.Tools.FileManager.Implementations;
using System.Text;

namespace FuelMeasurement.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private Thread _splashThread;
        private static ILogger _logger;
        protected override Window CreateShell()
        {
            _splashThread = new Thread(ShowSplash) { IsBackground = true };
            _splashThread.SetApartmentState(ApartmentState.STA);
            _splashThread.Start();

            void ShowSplash()
            {
                Container.Resolve<Splash>().Show();
                System.Windows.Threading.Dispatcher.Run();
                _logger.Info("App started");
            }

            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            _logger = LogManager.GetCurrentClassLogger();
            containerRegistry.RegisterInstance<ILogger>(_logger);

            containerRegistry.RegisterInstance<IEventAggregator>(new EventAggregator());

            containerRegistry.RegisterInstance<IMapper>(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ClientProfile());
            }).CreateMapper());

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            containerRegistry.RegisterSingleton<IDialogServices, DialogServices>();
            containerRegistry.RegisterSingleton<IFileManager, FileManager>();
            containerRegistry.RegisterSingleton<IXMLManager, XMLManager>();
            containerRegistry.RegisterSingleton<IProjectPathManager, ProjectPathManager>();
            containerRegistry.RegisterSingleton<IFileFilterManager, FileFilterManager>();

            containerRegistry.RegisterDialogWindow<MetroDialogWindow>();
            containerRegistry.RegisterDialog<InputTextDialogView, InputTextDialogViewModel>();
            containerRegistry.RegisterDialog<ConfirmationDialogView, ConfirmationDialogViewModel>();
            containerRegistry.RegisterDialog<NotificationDialogView, NotificationDialogViewModel>();

            containerRegistry.RegisterSingleton<IWaitWindowService, WaitWindowService>();

            containerRegistry.RegisterSingleton<IRepository, Repository>();
            
            containerRegistry.RegisterSingleton<ISelectionManager, SelectionManager>();

            containerRegistry.RegisterSingleton<ITriFormatFileReader, TriFormatFileReader>();
            containerRegistry.RegisterSingleton<ITriFormatFileWriter, TriFormatFileWriter>();

            containerRegistry.RegisterSingleton<ITxtFormatFileReader, TxtFormatFileReader>();
            containerRegistry.RegisterSingleton<ITxtFormatFileWriter, TxtFormatFileWriter>();

            containerRegistry.RegisterSingleton<IReadGeometryFileController, ReadGeometryFileController>();
            containerRegistry.RegisterSingleton<IWriteGeometryFilesController, WriteGeometryFilesController>();

            containerRegistry.RegisterSingleton<IGeometryRepository, GeometryRepository>();
            containerRegistry.RegisterSingleton<IViewportElementService, ViewportElementService>();
            containerRegistry.RegisterSingleton<IViewportElementManipulationService, ViewportElementManipulationService>();

            containerRegistry.RegisterSingleton<IHitTestService, HitTestService>();


            // Контроллер создания модели вычислителя
            containerRegistry.RegisterSingleton<ICalculationController, CalculationController>(); // Задействовано
            // Контроллер тарировщика
            containerRegistry.RegisterSingleton<ITaringController, TaringController>(); // Задействовано
            // Контроллер расчетчика
            containerRegistry.RegisterSingleton<IComputationController, ComputationController>();
            containerRegistry.RegisterSingleton<IMeasureController, MeasureController>();
            containerRegistry.RegisterSingleton<IErrorsController, ErrorsController>();
            containerRegistry.RegisterSingleton<ICommonMirrorController, CommonMirrorController>();
            containerRegistry.RegisterSingleton<ICapacityController, CapacityController>();
            containerRegistry.RegisterSingleton<IReferencedController, ReferencedController>();
            containerRegistry.Register<IPlotterController, PlotterController>();


            containerRegistry.Register<IReportController,     ReportController>();     // Общий генератор
            containerRegistry.Register<IReportModelCreator,   ReportModelCreator>();   // Создание модели отчета в Word
            containerRegistry.Register<IWordReportController, WordReportController>(); // Генератор отчетов по самолету в Word
            containerRegistry.Register<IBitmapHelper,   BitmapReportHelper>();   // Построитель изображений

            // Для настроек
            containerRegistry.RegisterSingleton<ISettingsTreeViewItemsHelper, SettingsTreeViewItemHelper>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            var dllNames = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*Module.dll").ToList();

            foreach (var dllName in dllNames)
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllName);

                    foreach (var type in assembly.GetTypes().Where(type => typeof(IModule).IsAssignableFrom(type)))
                    {
                        var moduleName = type.GetCustomAttribute<ModuleAttribute>()?.ModuleName;

                        if (moduleName == null)
                        {
                            continue;
                        }

                        moduleCatalog.AddModule(
                            new ModuleInfo
                            {
                                ModuleName = moduleName,
                                ModuleType = type.AssemblyQualifiedName,
                                Ref = new Uri(dllName, UriKind.RelativeOrAbsolute).AbsoluteUri,
                                //InitializationMode = InitializationMode.WhenAvailable
                            });
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"{ex}");
                    continue;
                }
            }

            return;
        }

        /// <summary>
        /// Инициализация модулей
        /// </summary>
        protected override void InitializeModules()
        {
            var moduleManager = Container.Resolve<IModuleManager>();
            var splashMessageEvent = Container.Resolve<IEventAggregator>().GetEvent<SplashMessage>();

            splashMessageEvent.Publish("Инициализация модулей");

            moduleManager.LoadModuleCompleted
                += (sender, args) =>
                {
                    splashMessageEvent.Publish($"{args.ModuleInfo.ModuleName} загружен");
                };

            var moduleCatalog = Container.Resolve<IModuleCatalog>();
            moduleCatalog.Initialize();

            splashMessageEvent.Publish("Инициализация модулей завершена");

            base.InitializeModules();
            return;
        }

        protected override void OnInitialized()
        {
            var splashMessageEvent = Container.Resolve<IEventAggregator>().GetEvent<SplashMessage>();
            splashMessageEvent.Publish($"Загрузка модулей завершена");
            Container.Resolve<IEventAggregator>().GetEvent<CloseSplashEvent>().Publish();

            base.OnInitialized();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            _logger.Info("App close");
        }
    }
}
