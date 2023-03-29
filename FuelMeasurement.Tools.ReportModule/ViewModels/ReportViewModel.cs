using NLog;
using FuelMeasurement.Common.Enums;
using Prism.Events;
using FuelMeasurement.Tools.CalculationData.Models;
using FuelMeasurement.Tools.ReportModule.Interfaces;
using FuelMeasurement.Tools.TaringModule.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Controls;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;
using FuelMeasurement.Client.UIModule.Models;
using System;
using FuelMeasurement.Data.Repositories.Repositories.Interfaces;
using System.Linq;
using FuelMeasurement.Tools.Plotter.Interfaces;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using FuelMeasurement.Model.Models.GeometryModels;
using FuelMeasurement.Tools.ReportModule.Models;
using FuelMeasurement.Common.DialogModule.Interfaces;
using FuelMeasurement.Common.SupportedFileFormats;
using System.Threading.Tasks;
using System.Windows.Media;
using SharpDX;
using System.Collections.Concurrent;
using FuelMeasurement.Tools.Compute.Interfaces;
using FuelMeasurement.Client.UIModule.Services.Interfaces;
using FuelMeasurement.Model.DTO.Models.ProjectModels;
using FuelMeasurement.Model.DTO.Models.AirplaneModels;

namespace FuelMeasurement.Tools.ReportModule.ViewModels
{
    /// <summary>
    /// Модель представления генератора отчетов
    /// </summary>
    public class ReportViewModel : BindableBase
    {
        #region Константы
        private const double Distance = 15_000;
        private const double AnimationTime = 500;
        private const string ReportFilter = $" Document Word | *{FileDOCX.Extension} | Document Word 97-2003 | *{FileDOC.Extension} | All Files | *.*";
        private const string ReportConfirmationTitle = $"Статус создания отчета";
        private const string ReportSuccess = $"Отчет успешно сгенерирован!";
        private const string ReportFailed = $"При генерации отчета что-то пошло не так! Попробуйте еще раз!";
        #endregion

        #region Интерфейсы
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICalculationController _calculationController; // На всякий случай
        private readonly IReportController _reportController;
        private readonly IPlotterController _plotterController;
        private readonly IComputationController _computationController;
        private readonly IBitmapHelper _bitmapReportHelper;
        private readonly IGeometryRepository _geometryRepository;
        private readonly IDialogServices _dialogServices;
        private readonly IMaterialService _materialService;
        private readonly IRepository _repository;
        private readonly IIndentService _indentService;
        private IEffectsManager _effectsManager = new DefaultEffectsManager();
        #endregion

        #region Команды
        public DelegateCommand<object> OnCanvasViewLoaded { get; private set; }
        public DelegateCommand OnCanvasSizeChanged { get; private set; }
        public DelegateCommand<object> OnViewportViewLoaded { get; private set; }
        public DelegateCommand OnGenerateReport { get; private set; }
        #endregion

        #region Приватные поля
        private readonly Dispatcher _dispatcher;
        private CalculationModel _calculationModel;
        private bool _relative;
        private Canvas _canvas;
        private HelixToolkit.Wpf.SharpDX.PerspectiveCamera _camera = new();
        private ObservableCollection<FuelTankMesh> _fuelTanks = new();
        private ObservableCollection<FuelTankGeometryModel> _geometryList;
        #endregion

        private List<ImageReportModel> TankViewList = new();

        #region Свойства
        /// <summary>
        /// Вьюпорт, в котором отображается топливный бак
        /// </summary>
        public Viewport3DX Viewport3Dx { get; set; }
        
        /// <summary>
        /// Камера, направленная на топливный бак
        /// </summary>
        public HelixToolkit.Wpf.SharpDX.PerspectiveCamera Camera
        {
            get => _camera;
            set => SetProperty(ref _camera, value);
        }
        
        public IEffectsManager EffectsManager
        {
            get => _effectsManager;
            protected set => SetProperty(ref _effectsManager, value);
        }

        /// <summary>
        /// Модель вычислителя
        /// </summary>
        public CalculationModel CalculationModel
        {
            get => _calculationModel;
            set => SetProperty(ref _calculationModel, value);

        }

        public ConfigurationModelDTO CurrentConfiguration
        {
            get => _currentConfiguration;
            protected set => SetProperty(ref _currentConfiguration, value);
        }
        private ConfigurationModelDTO _currentConfiguration;

        public ObservableCollection<FuelSensorMesh> FuelSensors
        {
            get => _fuelSensors;
            protected set => SetProperty(ref _fuelSensors, value);
        }
        private ObservableCollection<FuelSensorMesh> _fuelSensors = new();

        public ObservableCollection<FuelTankMesh> FuelTanks
        {
            get => _fuelTanks;
            protected set => SetProperty(ref _fuelTanks, value);
        }

        public ObservableCollection<FuelTankGeometryModel> GeometryList
        {
            get => _geometryList;
            protected set => SetProperty(ref _geometryList, value);
        }
        
        /// <summary>
        /// Заголовок окна
        /// </summary>
        public string WindowHeader { get; }

        /// <summary>
        /// Относительное или абсолютное отображение тарировочной кривой
        /// </summary>
        public bool Relative
        {
            get => _relative;
            set
            {
                if (_relative == value) return;
                _relative = value;
                RaisePropertyChanged(nameof(Relative));
                //TODO Здесь вызов смены парадигмы отображения тарировочной кривой
                _plotterController.FillTaringCanvas(_canvas, CalculationModel, 
                                                    Relative, null, CalculationModel.CurrentBranch.AnglesModel.ReferencedPitch,
                                                    CalculationModel.CurrentBranch.AnglesModel.ReferencedRoll);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор VM
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="calculationController"></param>
        /// <param name="reportController"></param>
        /// <param name="plotterController"></param>
        /// <param name="bitmapReportHelper"></param>
        /// <param name="geometryRepository"></param>
        public ReportViewModel(ILogger logger, IEventAggregator eventAggregator,
                               ICalculationController calculationController,
                               IReportController reportController,
                               IPlotterController plotterController,
                               IComputationController computationController,
                               IBitmapHelper bitmapReportHelper,
                               IGeometryRepository geometryRepository,
                               IMaterialService materialService,
                               IRepository repository,
                               IIndentService indentService,
                               IDialogServices dialogServices)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _calculationController = calculationController ?? throw new ArgumentNullException(nameof(calculationController));
            _reportController = reportController ?? throw new ArgumentNullException(nameof(reportController));
            _plotterController = plotterController ?? throw new ArgumentNullException(nameof(plotterController));
            _computationController = computationController ?? throw new ArgumentNullException(nameof(computationController));
            _bitmapReportHelper = bitmapReportHelper ?? throw new ArgumentNullException(nameof(bitmapReportHelper));
            _geometryRepository = geometryRepository ?? throw new ArgumentNullException(nameof(geometryRepository));
            _materialService = materialService ?? throw new ArgumentNullException(nameof(materialService));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _indentService = indentService ?? throw new ArgumentNullException(nameof(indentService));
            _dialogServices = dialogServices ?? throw new ArgumentNullException(nameof(dialogServices));

            CalculationModel = _calculationController.GetCalculationModel();

            _dispatcher = Dispatcher.CurrentDispatcher;

            switch (CalculationModel.CurrentReport)
            {
                case ReportObjectsEnum.TankReport:
                    WindowHeader = "Генератор отчетов для топливного бака";

                    break;
                case ReportObjectsEnum.AirplaneReport:
                    WindowHeader = "Генератор отчетов для самолета";

                    break;
            }
            
            InitializeCommand();
            // Инициализация графопостроителя
            GeometryList = new ObservableCollection<FuelTankGeometryModel>(_geometryRepository.GetAllGeometry());

            CurrentConfiguration = _repository
                .GetCurrentProject()
                .Configurations
                .FirstOrDefault(x =>
                x.Type == ConfigurationType.Working
                );
        }

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitializeCommand()
        {
            OnCanvasViewLoaded   = new DelegateCommand<object>(CanvasViewLoaded);
            OnCanvasSizeChanged  = new DelegateCommand(CanvasSizeChanged);
            OnViewportViewLoaded = new DelegateCommand<object>(ViewportViewLoaded);
            OnGenerateReport     = new DelegateCommand(GenerateReport);
        }

        /// <summary>
        /// Загрузка графика тарировочной кривой
        /// </summary>
        /// <param name="element"></param>
        private void CanvasViewLoaded(object element)
        {
            _canvas = element as Canvas;
            try
            {
                // Вызываем инициализатор графопостроителя
                _computationController.GraphicsInitialize(CalculationModel, GeometryList);
                if (_canvas != null) _plotterController.FillTaringCanvas(_canvas, CalculationModel, Relative, null,
                                                           CalculationModel.CurrentBranch.AnglesModel.ReferencedPitch,
                                                           CalculationModel.CurrentBranch.AnglesModel.ReferencedRoll);
            }
            catch(Exception ex)
            {
                _logger.Error($"Ошибка в окне генератора отчетов: {ex}");
            }
        }

        private void CanvasSizeChanged()
        {
            try
            {
                if (_canvas != null) _plotterController.FillTaringCanvas(_canvas, CalculationModel, 
                                     Relative, null, CalculationModel.CurrentBranch.AnglesModel.ReferencedPitch,
                                     CalculationModel.CurrentBranch.AnglesModel.ReferencedRoll);
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка в окне генератора отчетов: {ex}");
            }
        }

        /// <summary>
        /// Загрузка топливного бака
        /// </summary>
        /// <param name="element"></param>
        private async void ViewportViewLoaded(object element)
        {
            Viewport3Dx = element as Viewport3DX;
            if (Viewport3Dx != null)
            {
                await ActionInViewModel(() =>
                {
                    var material = _materialService.CreateMaterial(Colors.Silver, 0.3f);

                    foreach (var geometry in GeometryList)
                    {
                        //switch (geometry.Type)
                        //{
                        //    case TankGeometryType.TankGeometry:
                        //        FuelTanks.Add(new FuelTankMesh(
                        //            material,
                        //            material,
                        //            geometry
                        //            ));

                        //        break;

                        //    //case TankGeometryType.InsideModelGeometry:
                        //    //    InsideModelFuelTanks.Add(new InsideModelFuelTankMesh(
                        //    //        insideMaterial,
                        //    //        insideMaterial,
                        //    //        tankGeometry
                        //    //        ));

                        //}

                        var tankDTO = CurrentConfiguration.FuelTanks.FirstOrDefault(x => x.Id == geometry.Id);

                        if (tankDTO != null && tankDTO is FuelTankModelDTO tankModel)
                        {
                            tankModel.Sensors.ToList().ForEach(sensorDTO =>
                            {
                                var fuelSensorMesh = new FuelSensorMesh(
                                    sensorDTO.Id,
                                    sensorDTO.Name,
                                    PhongMaterials.Green,
                                    new Vector3(sensorDTO.UpPoint.X, sensorDTO.UpPoint.Y, sensorDTO.UpPoint.Z),
                                    new Vector3(sensorDTO.DownPoint.X, sensorDTO.DownPoint.Y, sensorDTO.DownPoint.Z),
                                    new Vector3(sensorDTO.InTankUpPoint.X, sensorDTO.InTankUpPoint.Y, sensorDTO.InTankUpPoint.Z),
                                    new Vector3(sensorDTO.InTankDownPoint.X, sensorDTO.InTankDownPoint.Y, sensorDTO.InTankDownPoint.Z),
                                    _indentService,
                                    tankDTO.Id
                                    );

                                FuelSensors.Add(fuelSensorMesh);
                            });
                        }

                        FuelTanks.Add(FillViewport3D(geometry, Camera));
                    }


                    var points = FuelTanks.ToList().Select(tank => tank.Geometry.BoundingSphere.Center);

                    var center = new Vector3D(points.Average(point => point.X),
                        points.Average(point => point.Y),
                        points.Average(point => point.Z));

                    Viewport3Dx?.LookAt(new Point3D(center.X, center.Y, center.Z), Distance, AnimationTime);
                });
            }
        }

        /// <summary>
        /// Метод генерации отчетов - буду заполнять в самую последнюю очередь
        /// </summary>
        private async void GenerateReport()
        {
            string path = string.Empty;

            _dialogServices.SaveFileDialog(ReportFilter, null, filepath =>
            {
                if (!string.IsNullOrWhiteSpace(filepath))
                {
                    path = filepath;
                }
            });

            TankViewList?.Clear();
            //TODO Надо сделать
            await ActionInViewModel(() =>
            {
                switch (CalculationModel.CurrentReport)
                {
                    case ReportObjectsEnum.TankReport:
                        if (FuelTanks.Count != 1)
                        {
                            MessageBox.Show("Предпринята попытка сгенерировать отчет по баку при загруженном самолете!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        Viewport3Dx.EffectsManager = EffectsManager;
                        TankViewList.Add(_bitmapReportHelper.TankViewToBitmap(Viewport3Dx.RenderBitmap()));
                        
                        _reportController.CreateTankReport(TankViewList, _canvas, ReportTypeEnum.WordReport, _calculationModel, _calculationModel.SelectedTanks, path);

                        _dialogServices.ShowNotificationDialog(ReportConfirmationTitle, ReportSuccess);

                        break;
                    case ReportObjectsEnum.AirplaneReport:
                        for (int t = 0; t < GeometryList.Count; t++)
                        {
                            FuelTanks[t].Visibility = Visibility.Hidden;
                        }
                        Viewport3Dx.RenderBitmap();

                        TankViewList?.Clear();
                        for (int i = 0; i < GeometryList.Count; i++)
                        {
                            if (i == 0)
                            {
                                FuelTanks[i].Visibility = Visibility.Visible;
                                var points = FuelTanks[i].Geometry.BoundingSphere.Center;

                                var center = new Vector3D(points.X,
                                                          points.Y,
                                                          points.Z);

                                Viewport3Dx?.LookAt(new Point3D(center.X, center.Y, center.Z), Distance, AnimationTime);
                                Viewport3Dx.EffectsManager = EffectsManager;
                                TankViewList?.Add(_bitmapReportHelper.TankViewToBitmap(Viewport3Dx.RenderBitmap()));
                            }
                            else
                            {
                                FuelTanks[i - 1].Visibility = Visibility.Hidden;
                                FuelTanks[i].Visibility = Visibility.Visible;
                                var points = FuelTanks[i].Geometry.BoundingSphere.Center;

                                var center = new Vector3D(points.X,
                                                          points.Y,
                                                          points.Z);

                                Viewport3Dx?.LookAt(new Point3D(center.X, center.Y, center.Z), Distance, AnimationTime);
                                Viewport3Dx.EffectsManager = EffectsManager;
                                TankViewList?.Add(_bitmapReportHelper.TankViewToBitmap(Viewport3Dx.RenderBitmap()));
                            }
                        }

                        _reportController.CreateAirplaneReport(TankViewList, _canvas, ReportTypeEnum.WordReport, CalculationModel, CalculationModel.SelectedTanks, path);

                        SetInitialState();

                        _dialogServices.ShowNotificationDialog(ReportConfirmationTitle, ReportSuccess);

                        break;
                }
            });
        }

        private void SetInitialState()
        {
            // Возвращаем видовой экран в исходное состояние
            for (int t = 0; t < GeometryList.Count; t++)
            {
                FuelTanks[t].Visibility = Visibility.Visible;
            }
            var points = FuelTanks.ToList().Select(tank => tank.Geometry.BoundingSphere.Center);

            var center = new Vector3D(points.Average(point => point.X),
                points.Average(point => point.Y),
                points.Average(point => point.Z));

            Viewport3Dx?.LookAt(new Point3D(center.X, center.Y, center.Z), Distance, AnimationTime);
            Viewport3Dx.RenderBitmap();
        }

        public async Task ActionInViewModel(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
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
                        _logger.Error($"Ошибка в окне генератора отчетов: {ex}");
                    }
                }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public FuelTankMesh FillViewport3D(FuelTankGeometryModel geometry, HelixToolkit.Wpf.SharpDX.PerspectiveCamera camera)
        {
            camera.Position = new Point3D(0.5, 5, 0);
            camera.NearPlaneDistance = 0.1f;
            camera.FarPlaneDistance = 100000;
            camera.UpDirection = new Vector3D(-1, 0.5, 0);
            camera.LookDirection = new Vector3D(-1, -2, -0);

            FuelTankMesh mesh = new(GetPhongMaterial(Colors.DarkBlue.ToColor4(), 0.3f),
                                    GetPhongMaterial(Colors.Cyan.ToColor4(), 0.3f),
                                    geometry);

            return mesh;
        }

        private readonly ConcurrentDictionary<Color4, PhongMaterial> _phongMaterials = new();

        public PhongMaterial GetPhongMaterial(Color4 color, float opacity)
        {
            return
                _phongMaterials.TryGetValue(color, out var phongMaterial)
                    ? phongMaterial
                    : _phongMaterials.GetOrAdd(
                        color,
                        new PhongMaterial
                        {
                            AmbientColor = PhongMaterials.ToColor(0.1, 0.1, 0.1, 1.0),
                            DiffuseColor = new Color4(color.Green, color.Blue, color.Red, opacity),
                            SpecularColor = PhongMaterials.ToColor(0.0225, 0.0225, 0.0225, 1.0),
                            EmissiveColor = PhongMaterials.ToColor(0.0, 0.0, 0.0, 1.0),
                            SpecularShininess = 12.8f,
                            RenderShadowMap = false,
                            EnableFlatShading = false
                        }
                    );
        }
    }
}