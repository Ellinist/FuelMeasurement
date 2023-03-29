using ControlzEx.Theming;
using DryIoc;
using FuelMeasurement.Common.DialogModule.Interfaces;
using FuelMeasurement.Client.Tools;
using FuelMeasurement.Client.UIModule.ViewModels;
using FuelMeasurement.Client.UIModule.Views;
using FuelMeasurement.Client.Views;
using FuelMeasurement.Client.WaitWindowModule.Interfaces;
using FuelMeasurement.Common.Constants;
using FuelMeasurement.Common.Enums;
using FuelMeasurement.Common.Events;
using FuelMeasurement.Common.Events.ProjectEvents;
using FuelMeasurement.Common.Models;
using FuelMeasurement.Data.Repositories.Repositories.Interfaces;
using FuelMeasurement.Model.DTO.Models.AirplaneModels;
using FuelMeasurement.Model.DTO.Models.ProjectModels;
using FuelMeasurement.Model.Models.GeometryModels;
using FuelMeasurement.Tools.CalculationData.Models;
using FuelMeasurement.Tools.Compute.Interfaces;
using FuelMeasurement.Tools.Geometry.Interfaces;
using FuelMeasurement.Tools.ReportModule.Views;
using FuelMeasurement.Tools.TaringModule.Interfaces;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FuelMeasurement.Client.WaitWindowModule.Events;
using FuelMeasurement.Common.SettingsModule.Views;
using FuelMeasurement.Tools.FileManager.Interfaces;

namespace FuelMeasurement.Client.ViewModels
{
    internal class MenubarViewModel :  BindableBase, IDisposable
    {
        private readonly IDialogServices _dialogServices;
        private readonly IRegionManager _regionManager;
        private readonly IContainer _container;
        private readonly IWaitWindowService _waitWindowService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly IReadGeometryFileController _readController;
        private readonly IWriteGeometryFilesController _writeController;
        private readonly IGeometryRepository _geometryRepository;
        private readonly IRepository _repository;
        private readonly IFileManager _fileManager;


        #region Интерфейсы вычислителя и расчетчикв
        private readonly ICalculationController _calculationController;
        private readonly ITaringController _taringController;
        private readonly IComputationController _computationController;
        #endregion

        private bool _disposed = false;

        private IRegion _region;
        private CreateNewProjectView _createNewProject;

        private readonly Dispatcher _dispatcher;

        private readonly SubscriptionToken _mainWindowLoadedToken;
        private readonly SubscriptionToken _closeViewToken;
        private readonly SubscriptionToken _cancelCloseViewToken;
        private readonly SubscriptionToken _loadProjectToken;
        private readonly SubscriptionToken _cancelledToken;

        private bool _activateStopTrigger = false; // Флаг остановки цикла по бакам
        public bool ActivateStopTrigger
        {
            get => _activateStopTrigger;
            set => SetProperty(ref _activateStopTrigger, value);
        }

        private const string _xmlFilter = "XML| *.xml";

        /// <summary>
        /// Перечень открытых окон.
        /// </summary>
        private readonly List<Window> Wnds = new();

        private CalculationModel _currentModel;

        private bool _canOpenEditor3D = false;
        public bool CanOpenEditor3D
        {
            get => _canOpenEditor3D;
            set => SetProperty(ref _canOpenEditor3D, value);
        }


        private bool _enableMenu = true;
        public bool EnableMenu
        {
            get => _enableMenu;
            set => SetProperty(ref _enableMenu, value);
        }

        public List<AccentColorMenuData> AccentColors { get; set; }
        //public List<string> TankIdList { get; set; } = new();

        #region Commands Region
        public DelegateCommand CreateProjectCommand { get; private set; }
        public DelegateCommand LoadProjectXMLCommand { get; private set; }
        public DelegateCommand LoadProjectDBCommand { get; private set; }
        public DelegateCommand SaveProjectXMLCommand { get; private set; }
        public DelegateCommand SaveProjectDBCommand { get; private set; }
        public DelegateCommand AboutProgramCommand { get; private set; }
        public DelegateCommand OpenEditor3DCommand { get; private set; }
        public DelegateCommand OpenSettingsCommand { get; private set; }
        public DelegateCommand LoadProjectZIPCommand { get; private set; }

        public DelegateCommand ChangeMainThemeToLightCommand { get; private set; }
        public DelegateCommand ChangeMainThemeToDarkCommand { get; private set; }

        public DelegateCommand ShowTestWindowCommand { get; private set; }

        /// <summary>
        /// Команда создания отчета по топливному баку
        /// </summary>
        public DelegateCommand CreateTankReportCommand { get; private set; }

        /// <summary>
        /// Команда создания отчета по самолету
        /// </summary>
        public DelegateCommand CreateAirplaneReportCommand { get; private set; }
        #endregion



        public MenubarViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public MenubarViewModel(
            IDialogServices dialogServices,
            IRegionManager regionManager,
            IContainer container,
            IEventAggregator eventAggregator,
            ILogger logger,
            IReadGeometryFileController readController,
            IWriteGeometryFilesController writeController,
            IGeometryRepository geometryRepository,
            ICalculationController calculationController,
            ITaringController taringController,
            IComputationController computationController , // Временно
            IWaitWindowService waitWindowService,
            IRepository repository,
            IFileManager fileManager
            ) : this()
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _dialogServices = dialogServices ?? throw new ArgumentNullException(nameof(dialogServices));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _readController = readController ?? throw new ArgumentNullException(nameof(readController));
            _writeController = writeController ?? throw new ArgumentNullException(nameof(writeController));
            _geometryRepository = geometryRepository ?? throw new ArgumentNullException(nameof(geometryRepository));
            _waitWindowService = waitWindowService ?? throw new ArgumentNullException(nameof(waitWindowService));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));

            #region Для вычислителя и расчетчика
            _calculationController = calculationController ?? throw new ArgumentNullException(nameof(calculationController));
            _taringController = taringController ?? throw new ArgumentNullException(nameof(taringController));
            _computationController = computationController ?? throw new ArgumentNullException(nameof(computationController));
            #endregion

            _mainWindowLoadedToken = _eventAggregator.GetEvent<MainWindowLoadedEvent>().Subscribe(OnMainWindowLoaded);
            _closeViewToken = _eventAggregator.GetEvent<CloseViewEvent>().Subscribe(OnSuccsessCloseView);
            _cancelledToken = _eventAggregator.GetEvent<WaitCancelledEvent>().Subscribe(StopCycle);

            _loadProjectToken = _eventAggregator.GetEvent<ProjectLoaded>().Subscribe(async (project) =>
            {
                await Task.Run(async () =>
                {
                    await OnProjectLoaded(project);
                });
            });

            InitializeCommand();
            InitializeThemesLists();
        }

        private void InitializeThemesLists()
        {
            // create accent color menu items
            this.AccentColors = ThemeManager.Current.Themes               
                .GroupBy(x => x.ColorScheme)
                .OrderBy(a => a.Key)
                .Select(a => new AccentColorMenuData 
                {
                    Name = a.Key, 
                    ColorBrush = a.First().ShowcaseBrush 
                })
                .ToList();
        }

        private async Task OnProjectLoaded(object obj)
        {
            if (obj is ProjectModelDTO project)
            {
                _repository.SetCurrentProject(project);
                _geometryRepository.RemoveAllGeometry();

                var currentConfiguration = project.Configurations
                    .FirstOrDefault(configuration => configuration.Type == ConfigurationType.Working);

                var projectBranch = currentConfiguration.Branches
                    .FirstOrDefault(branch => branch.Type == BranchType.Working);

                if (projectBranch != null)
                {
                    _currentModel = _calculationController.CalculationInitialize(projectBranch); // Инициализация вычислителя

                    foreach (var tank in currentConfiguration.FuelTanks)
                    {
                        if (!ActivateStopTrigger)
                        {
                            var tankGeometry = await _readController.ReadGeometry(tank.GeometryFilePath);

                            if (tankGeometry != null)
                            {
                                tankGeometry.Id = tank.Id;
                                //TankIdList.Add(tankGeometry.Id);
                                tankGeometry.Type = TankGeometryType.TankGeometry;

                                _waitWindowService.AddMessage($"Читаю файл бака - {tank.Name} - Успешно!");
                                if (_geometryRepository.AddTankGeometry(tankGeometry.Id, tankGeometry))
                                {
                                    await CreateHeatMap(tank);
                                    await AddTankInModel(tank, tankGeometry, projectBranch.Configuration.Coefficient);
                                }
                                else
                                {
                                    _waitWindowService.AddMessage($"Не удалось загрузить геометрию бака - {tank.Name} в репозиторий");
                                }
                            }
                            else
                            {
                                _waitWindowService.AddMessage("Не удалось загрузить геометрию. Нет рабочей ветки.");
                            }
                        }
                        else break; // Если принудительно остановлен процесс тарировки, то выходим из цикла
                    }

                    // Здесь продумать, надо ли чистить списки тарировки при отмене на каком либо баке
                    #region Возвращение исходного состояния
                    if (ActivateStopTrigger)
                    {
                        // Чистим тарировочные данные в случае отмены процесса тарировки
                        // Пока так - надо думать, может потом сделать флаг для оттарированных баков, чтобы не тарировать их дважды
                        foreach(var t in _currentModel.BranchTanks)
                        {
                            t.TarResultList.Clear();
                        }

                        await _dispatcher.BeginInvoke(() =>
                        {
                            _dialogServices.ShowNotificationDialog("Процесс тарировки", "Процесс тарировки отменен пользователем!\n Можете попробовать еще раз!");
                        });
                        ActivateStopTrigger = false;
                        _cancelledToken.Dispose();
                    }
                    #endregion

                    foreach (var insideModel in currentConfiguration.InsideModelFuelTanks)
                    {
                        var tankGeometry = await _readController.ReadGeometry(insideModel.GeometryFilePath);

                        if (tankGeometry != null)
                        {
                            tankGeometry.Id = insideModel.Id;
                            tankGeometry.Type = TankGeometryType.InsideModelGeometry;

                            _waitWindowService.AddMessage($"Читаю файл внутренностей бака - {insideModel.Name} - Успешно!");
                            if (!_geometryRepository.AddTankGeometry(tankGeometry.Id, tankGeometry))
                            {
                                _waitWindowService.AddMessage($"Не удалось загрузить геометрию внутренностей - {insideModel.Name} в репозиторий");
                            }
                        }
                        else
                        {
                            _waitWindowService.AddMessage("Не удалось загрузить геометрию внутренностей. Нет рабочей ветки.");
                        }
                    }

                    CanOpenEditor3D = true;
                    EnableMenu = true;
                }

                _waitWindowService.Close();
            }
        }


        /// <summary>
        /// Добавление расчетной модели топливного бака и тарировка по условиям
        /// </summary>
        /// <param name="tank"></param>
        /// <param name="tankGeometry"></param>
        /// <returns></returns>
        private Task AddTankInModel(TankModelDTO tank, FuelTankGeometryModel tankGeometry, float k)
        {
            if (tankGeometry.Mesh.TankType == MeshType.FuelTank)
            {
                _waitWindowService.AddMessage($"Расчет тарировки бака - {tank.Name}");
                // тут далаем расчёт тарировки и т.д.
                _taringController.AddTankModel(_currentModel, tank.Id, tankGeometry.Mesh, tankGeometry.FuelTankName, k);
            }

            return Task.CompletedTask;
        }

        private Task CreateHeatMap(TankModelDTO tank)
        {
            _waitWindowService.AddMessage($"Создана тепловая карта бака - {tank.Name}");
            // тут будет тепловая карта

            return Task.CompletedTask;
        }

        private void OnMainWindowLoaded()
        {
            _region = _regionManager.Regions[RegionNames.WorkingRegion];
            _createNewProject = _container.Resolve<CreateNewProjectView>();
        }

        private void InitializeCommand()
        {
            CreateProjectCommand = new DelegateCommand(CreateNewProject);
            LoadProjectXMLCommand = new DelegateCommand(LoadProjectXML);
            LoadProjectDBCommand = new DelegateCommand(LoadProjectDB);
            LoadProjectZIPCommand = new DelegateCommand(LoadProjectZIP);
            SaveProjectXMLCommand = new DelegateCommand(SaveProjectXML, CanSaveProjectXML);
            SaveProjectDBCommand = new DelegateCommand(SaveProjectDB, CanSaveProjectDB);
            AboutProgramCommand = new DelegateCommand(AboutProgram);
            OpenSettingsCommand = new DelegateCommand(ShowSettingsView);
            
            OpenEditor3DCommand = new DelegateCommand(OpenView);

            ChangeMainThemeToDarkCommand = new DelegateCommand(ChangeMainThemeToDark);
            ChangeMainThemeToLightCommand = new DelegateCommand(ChangeMainThemeToLight);

            CreateTankReportCommand = new DelegateCommand(CreateTankReport);
            CreateAirplaneReportCommand = new DelegateCommand(CreateAirplaneReport);

            ShowTestWindowCommand = new DelegateCommand(() =>
            {
                new TestWindowView().Show();
                
            }
            );
        }

        private bool CanOpenSettingsView() => true;
        private void ShowSettingsView()
        {
            if (CanOpenSettingsView())
            {
                _region = _regionManager.Regions[RegionNames.WorkingRegion];
                var settingsRegion = _container.Resolve<AppSettingsView>();

                if (_region.Views.Any())
                {
                    _region.RemoveAll();
                }

                _region.Add(settingsRegion);
            }
        }

        #region Блок генераторов отчета
        private bool CanCreateTankReport()
        {
            //TODO здесь проверка на возможность создания отчета
            if (_currentModel == null) return false;
            else return true; // Пока заглушка
        }

        /// <summary>
        /// Создание генератора отчетов для топливного бака
        /// </summary>
        private void CreateTankReport()
        {
            if (CanCreateTankReport())
            {
                _currentModel.CurrentReport = ReportObjectsEnum.TankReport; // Указываем, что отчет для топливного бака
                _region = _regionManager.Regions[RegionNames.WorkingRegion];
                var reportRegion = _container.Resolve<ReportRegionView>();

                if (_region.Views.Any())
                {
                    _region.RemoveAll();
                }

                _region.Add(reportRegion);
            }
        }

        private bool CanCreateAirplaneReport()
        {
            //TODO здесь проверка на возможность создания отчета
            if (_currentModel == null) return false;
            else return true; // Пока заглушка
        }

        /// <summary>
        /// Создание генератора отчетов для самолета
        /// </summary>
        private void CreateAirplaneReport()
        {
            if (CanCreateAirplaneReport())
            {
                _currentModel.CurrentReport = ReportObjectsEnum.AirplaneReport; // Указываем, что отчет для самолета
                _region = _regionManager.Regions[RegionNames.WorkingRegion];
                var reportRegion = _container.Resolve<ReportRegionView>();

                if (_region.Views.Any())
                {
                    _region.RemoveAll();
                }

                _region.Add(reportRegion);
            }
        }
        #endregion

        private void ChangeMainThemeToLight()
        {
            ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Light");
        }

        private void ChangeMainThemeToDark()
        {
            ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Dark");
        }

        private void OpenView()
        {
            try
            {
                var window = new Viewer3DView();

                window.Closing += (_, _) =>
                {
                    ((Viewer3DViewModel)window.DataContext).Closed();
                };

                window.Closed
                += (_, _) =>
                {
                    Wnds.Remove(window);
                };

                Wnds.Add(window);
                window.Show();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        
        #region CreateNewProject Region
        private void CreateNewProject()
        {
            _region.RemoveAll();
            _region.Add(_createNewProject);
        }
        #endregion

        private void OnSuccsessCloseView(CloseViewParams obj)
        {
            switch (obj.Type)
            {
                case ViewType.CreateNewProjectView:

                    _region.RemoveAll();

                    if(!obj.IsCancle)
                    {
                        ShowWaitWindow();
                    }

                    break;
            }
        }

        private void ShowWaitWindow()
        {
            _waitWindowService.SecondaryProgressBar.IsIndeterminate = true;
            _waitWindowService.ClearMessage();
            _waitWindowService.Show(true,true);
        }

        #region XML Region
        private void SaveProjectXML()
        {
            _dialogServices.SaveFileDialog(
                _xmlFilter,
                "",
                file =>
                {

                });
        }

        private bool CanSaveProjectXML()
        {
            // Тут проверку добавить
            return true;
        }

        private void LoadProjectXML()
        {
            _dialogServices.OpenFileDialog(
                _xmlFilter,
                false,
                file => 
                {

                });
        }
        #endregion

        #region DB Region
        private void LoadProjectDB()
        {

        }

        private void SaveProjectDB()
        {

        }

        private bool CanSaveProjectDB()
        {
            // Добавить проверку

            return true;
        }
        #endregion

        #region ZIP

        private void LoadProjectZIP()
        {
            var result = _fileManager.LoadProjectZIP();
        }


        #endregion

        #region About Program Region
        private void AboutProgram()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            StringBuilder builder = new();
            builder.Append("ИРДТ - Интерактивная расстановка датчиков топливомера");
            builder.AppendLine();
            builder.Append("Версия: ");
            builder.Append(version);
            builder.AppendLine();
            builder.Append("Программировали: Гагарин Алексей, Голиней Вячеслав,");
            builder.AppendLine();
            builder.Append("Андрушенко Дмитрий и Киви Метёлкин");
            builder.AppendLine();
            builder.Append("(с) ПАО ТЕХПРИБОР 2020-2022");

            _dialogServices.ShowNotificationDialog(
                "О программе",
                builder.ToString()
                );
        }
        #endregion

        public void ActionInViewModel(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _dispatcher.Invoke(priority,
                new Action(() =>
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }));
        }

        public async Task ActionInViewModelAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            await _dispatcher.BeginInvoke(priority,
                new Action(() =>
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }));
        }



        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _closeViewToken?.Dispose();
                _mainWindowLoadedToken?.Dispose();
                _loadProjectToken?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void StopCycle()
        {
            if (!ActivateStopTrigger)
            {
                ActivateStopTrigger = true;
                //_cancelledToken.Dispose();
            }
        }

        ~MenubarViewModel()
        {
            Dispose(false);
        }
    }
}
