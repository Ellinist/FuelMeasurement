using FuelMeasurement.Common.Enums;
using FuelMeasurement.Data.Repositories.Repositories.Interfaces;
using FuelMeasurement.Client.UIModule.Infrastructure;
using FuelMeasurement.Client.UIModule.Models;
using FuelMeasurement.Client.UIModule.Services;
using FuelMeasurement.Client.UIModule.Services.Interfaces;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using NLog;
using Prism.Mvvm;
using DryIoc;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;
using Prism.Events;
using FuelMeasurement.Client.UIModule.Events;
using Prism.Commands;
using System.Collections.Generic;
using SharpDX;
using Prism.Regions;
using FuelMeasurement.Tools.TaringModule.Interfaces;
using FuelMeasurement.Tools.CalculationData.Models;
using System.Windows.Controls;
using FuelMeasurement.Tools.Plotter.Interfaces;
using ScottPlot;
using FuelMeasurement.Tools.Compute.Interfaces;
using FuelMeasurement.Model.DTO.Models.ProjectModels;
using FuelMeasurement.Model.DTO.Models.AirplaneModels;
using FuelMeasurement.Model.Models.GeometryModels;
using FuelMeasurement.Client.UIModule.UserControls;
using ScottPlot.Plottable;
using RenderType = ScottPlot.RenderType;
using FuelMeasurement.Client.UIModule.Infrastructure.Extensions;

namespace FuelMeasurement.Client.UIModule.ViewModels
{
    public class Viewer3DViewModel : BindableBase, IActionInViewModel
    {
        private const double DELTA_Y = 0.0001;
        private const double DELTA_X = 0.1;

        private Dispatcher _dispatcher;

        private readonly ILogger _logger;
        private readonly IGeometryRepository _geometryRepository;
        private readonly IViewportElementService _viewportElementService;
        private readonly IViewportElementManipulationService _viewportElementManipulationService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IIndentService _indentService;
        private readonly ICalculationController _calculationController;
        private readonly IPlotterController _plotterController;
        private readonly ICapacityController _capacityController;
        private readonly IComputationController _computationController;
        private readonly IHitTestService _hitTestService;
        private readonly IRepository _repository;
        private readonly IHeatMapService _heatMapService;
        private readonly IMaterialService _materialService;
        private readonly IPlotterSettingsService _plotterService;
        private readonly IRegionManager _regionManager;
        private readonly IContainer _container;

        #region Для слайдеров
        private double _minimumPitch;
        public double MinimumPitch
        {
            get => _minimumPitch;
            set
            {
                _minimumPitch = value;
                RaisePropertyChanged(nameof(MinimumPitch));
            }
        }
        private double _maximumPitch;
        public double MaximumPitch
        {
            get => _maximumPitch;
            set
            {
                _maximumPitch = value;
                RaisePropertyChanged(nameof(MaximumPitch));
            }
        }
        private double _minimumRoll;
        public double MinimumRoll
        {
            get => _minimumRoll;
            set
            {
                _minimumRoll = value;
                RaisePropertyChanged(nameof(MinimumRoll));
            }
        }
        private double _maximumRoll;
        public double MaximumRoll
        {
            get => _maximumRoll;
            set
            {
                _maximumRoll = value;
                RaisePropertyChanged(nameof(MaximumRoll));
            }
        }

        private ObservableCollection<TankPoint> _tankPoints = new();
        /// <summary>
        /// Список позиций баков при повороте углов
        /// </summary>
        public ObservableCollection<TankPoint> TankPoints
        {
            get => _tankPoints;
            set
            {
                if (_tankPoints == value) return;
                _tankPoints = value;
                RaisePropertyChanged(nameof(TankPoint));
            }
        }

        private double _pitchValue;
        public double PitchValue
        {
            get => _pitchValue;
            set
            {
                _pitchValue = value;
                RaisePropertyChanged(nameof(PitchValue));
                // Здесь будет вызов методов перерисовки графиков
                UpdateGraphics();
                SetTankPoints();
            }
        }
        private double _rollValue;
        public double RollValue
        {
            get => _rollValue;
            set
            {
                _rollValue = value;
                RaisePropertyChanged(nameof(RollValue));
                // Здесь будет вызов методов перерисовки графиков
                UpdateGraphics();
                SetTankPoints();
            }
        }
        #endregion

        /// <summary>
        /// Метод установки списка позиций верхних и нижних точек топливных баков при заданных углах
        /// </summary>
        private void SetTankPoints()
        {
            TankPoints.Clear();
            for (int p = 0; p < _calculationModel.SelectedTanks.Count; p++)
            {
                var currentData = _calculationModel.SelectedTanks[p].TarResultList.FirstOrDefault(x => x.Pitch == PitchValue && x.Roll == RollValue);

                TankPoint tp = new()
                {
                    TankName = _calculationModel.SelectedTanks[p].TankName,
                    UpY = currentData.ResultAxisY[^1],
                    DownY = currentData.ResultAxisY[0]
                };
                TankPoints.Add(tp);
            }
        }

        private BubblePlot CurrentPoint { get; set; }
        private BubblePlot CapacityPoint { get; set; }
        private readonly double[] _currentX = new double[1];
        private readonly double[] _currentY = new double[1];
        // Объявляем индекс выделения
        private int _lastHighlightedIndex = -1;
        private int _lastCapacityHighlightedIndex = -1;

        #region Свойства значений объемов и МПИ
        private double currentVolume;
        /// <summary>
        /// Текущий объем в точке курсора на графике МПИ
        /// </summary>
        public double CurrentVolume
        {
            get => currentVolume;
            set
            {
                if (Math.Abs(currentVolume - value) < DELTA_X) return;
                currentVolume = value;
                RaisePropertyChanged(nameof(CurrentVolume));
            }
        }

        private double currentError;
        /// <summary>
        /// Текущая погрешность в точке курсора на графике МПИ
        /// </summary>
        public double CurrentError
        {
            get => currentError;
            set
            {
                if (Math.Abs(currentError - value) < DELTA_Y) return;
                currentError = value;
                RaisePropertyChanged(nameof(CurrentError));
            }
        }

        private double nearestVolume;
        /// <summary>
        /// Ближайший объем на текущей кривой графика МПИ
        /// </summary>
        public double NearestVolume
        {
            get => nearestVolume;
            set
            {
                if (Math.Abs(nearestVolume - value) < DELTA_X) return;
                nearestVolume = value;
                RaisePropertyChanged(nameof(NearestVolume));
            }
        }

        private double nearestError;
        /// <summary>
        /// Ближайшая погрешность на текущей кривой графика МПИ
        /// </summary>
        public double NearestError
        {
            get => nearestError;
            set
            {
                if (Math.Abs(nearestError - value) < DELTA_Y) return;
                nearestError = value;
                RaisePropertyChanged(nameof(NearestError));
            }
        }
        #endregion

        #region Свойства графика погонной емкости
        private double capacityVolume;
        /// <summary>
        /// Текущий объем на графике погонной емкости
        /// </summary>
        public double CapacityVolume
        {
            get => capacityVolume;
            set
            {
                if (Math.Abs(capacityVolume - value) < DELTA_X) return;
                capacityVolume = value;
                RaisePropertyChanged(nameof(CapacityVolume));
            }
        }

        private double nearestCapacityVolume;
        /// <summary>
        /// Ближайший объем текущей кривой на графике погонной емкости
        /// </summary>
        public double NearestCapacityVolume
        {
            get => nearestCapacityVolume;
            set
            {
                if (Math.Abs(nearestCapacityVolume - value) < DELTA_X) return;
                nearestCapacityVolume = value;
                RaisePropertyChanged(nameof(NearestCapacityVolume));
            }
        }

        private double currentCapacity;
        /// <summary>
        /// Текущая емкость на графике погонной емкости
        /// </summary>
        public double CurrentCapacity
        {
            get => currentCapacity;
            set
            {
                if (Math.Abs(currentCapacity - value) < DELTA_Y) return;
                currentCapacity = value;
                RaisePropertyChanged(nameof(CurrentCapacity));
            }
        }

        private double nearestCapacity;
        /// <summary>
        /// Ближайшая емкость на текущей кривой на графике погонной емкости
        /// </summary>
        public double NearestCapacity
        {
            get => nearestCapacity;
            set
            {
                if (Math.Abs(nearestCapacity - value) < DELTA_Y) return;
                nearestCapacity = value;
                RaisePropertyChanged(nameof(NearestCapacity));
            }
        }
        #endregion

        #region Для радио-кнопок
        private ComputeTypeEnum _computeType = ComputeTypeEnum.ErrorsUsual;
        public ComputeTypeEnum ComputeType
        {
            get => _computeType;
            set
            {
                if (_computeType != value)
                {
                    _computeType = value;
                    RaisePropertyChanged(nameof(ComputeType));
                }
            }
        }
        #endregion

        #region Для графиков
        private CalculationModel _calculationModel;
        private Canvas _canvas; // График тарировочной кривой
        private WpfPlot _errorsPlot; // График МПИ
        private WpfPlot _capacityPlot; // График погонной емкости
        #endregion

        public Viewport3DX Viewport3Dx { get; set; }

        public ConfigurationModelDTO CurrentConfiguration
        {
            get => _currentConfiguration;
            protected set => SetProperty(ref _currentConfiguration, value);
        }
        private ConfigurationModelDTO _currentConfiguration;

        public HelixToolkit.Wpf.SharpDX.ProjectionCamera Camera
        {
            get => _camera;
            protected set => SetProperty(ref _camera, value);
        }
        private HelixToolkit.Wpf.SharpDX.ProjectionCamera _camera;

        public IEffectsManager EffectsManager
        {
            get => _effectsManager;
            protected set => SetProperty(ref _effectsManager, value);
        }
        private IEffectsManager _effectsManager;

        private string _title = "Расстановка датчиков";

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public Window Window { get; set; }
        public UserControl ComputeTypeControl { get; set; }

        public Point3D? MousePosition
        {
            get => _mousePosition;
            set
            {
                if (SetProperty(ref _mousePosition, value))
                {
                    if (MousePosition != null)
                    {
                        var p = (Point3D)MousePosition;
                        var x = Math.Round(p.X, 2, MidpointRounding.AwayFromZero);
                        var y = Math.Round(p.Y, 2, MidpointRounding.AwayFromZero);
                        var z = Math.Round(p.Z, 2, MidpointRounding.AwayFromZero);

                        MousePositionText = $"Позиция мыши: '{x}'|'{y}'|'{z}'";
                    }
                    else
                    {
                        MousePositionText = $"Позиция мыши: '-'|'-'|'-'";
                    }
                }
            }
        }
        private Point3D? _mousePosition = null;

        public string MousePositionText
        {
            get => _mousePositionText;
            protected set => SetProperty(ref _mousePositionText, value);
        }
        private string _mousePositionText = string.Empty;

        private ObservableCollection<FuelTankGeometryModel> _geometryList;
        public ObservableCollection<FuelTankGeometryModel> GeometryList
        {
            get => _geometryList;
            protected set => SetProperty(ref _geometryList, value);
        }

        public ObservableCollection<FuelTankMesh> FuelTanks
        {
            get => _fuelTanks;
            protected set => SetProperty(ref _fuelTanks, value);
        }
        private ObservableCollection<FuelTankMesh> _fuelTanks = new();

        public ObservableCollection<InsideModelFuelTankMesh> InsideModelFuelTanks
        {
            get => _insideModelFuelTanks;
            protected set => SetProperty(ref _insideModelFuelTanks, value);
        }
        private ObservableCollection<InsideModelFuelTankMesh> _insideModelFuelTanks = new();

        public ObservableCollection<FuelSensorMesh> FuelSensors
        {
            get => _fuelSensors;
            protected set => SetProperty(ref _fuelSensors, value);
        }
        private ObservableCollection<FuelSensorMesh> _fuelSensors = new();

        public ObservableCollection<CoordinateLineMesh> CoordinateLines
        {
            get => _coordinateLines;
            protected set => SetProperty(ref _coordinateLines, value);
        }
        private ObservableCollection<CoordinateLineMesh> _coordinateLines = new();

        public ObservableCollection<GridMesh> GridLines
        {
            get => _gridLines;
            protected set => SetProperty(ref _gridLines, value);
        }
        private ObservableCollection<GridMesh> _gridLines = new();

        public SubscriptionToken _switchModeToken;

        public DelegateCommand<string> SwitchTankVisibilityCommand
        {
            get => _switchTankVisibilityCommand;
            protected set => SetProperty(ref _switchTankVisibilityCommand, value);
        }
        private DelegateCommand<string> _switchTankVisibilityCommand;

        public DelegateCommand<string> NavigateOnTankCommand
        {
            get => _navigateOnTankCommand;
            protected set => SetProperty(ref _navigateOnTankCommand, value);
        }
        private DelegateCommand<string> _navigateOnTankCommand;





        public DelegateCommand<object> OnCanvasViewLoaded { get; private set; }
        public DelegateCommand OnCanvasSizeChanged { get; private set; }
        public DelegateCommand<object> OnErrorsTabLoaded { get; private set; }
        public DelegateCommand<object> OnCapacityTabLoaded { get; private set; }
        public DelegateCommand<object> UCOnLoaded { get; private set; }



        public DelegateCommand RemoveSelectedSensorCommand
        {
            get => _removeSelectedSensorCommand;
            protected set => SetProperty(ref _removeSelectedSensorCommand, value);
        }
        private DelegateCommand _removeSelectedSensorCommand;

        public ViewerWorkingModes ViewerWorkingMode
        {
            get => _viewerWorkingMode;
            set => SetProperty(ref _viewerWorkingMode, value);
        }
        private ViewerWorkingModes _viewerWorkingMode = ViewerWorkingModes.View;

        public FuelSensorMesh SelectedSensor
        {
            get => _selectedSensor;
            set
            {
                if (SetProperty(ref _selectedSensor, value))
                {
                    RaisePropertyChanged(nameof(HitSensorItemName));
                }
            }
        }
        private FuelSensorMesh _selectedSensor = null;

        public FuelSensorMesh HitSensor
        {
            get => _hitSensor;
            set
            {
                if (SetProperty(ref _hitSensor, value))
                {
                    RaisePropertyChanged(nameof(HitSensorItemName));
                }
            }
        }
        private FuelSensorMesh _hitSensor = null;

        public FuelTankMesh SelectedFuelTank
        {
            get => _selectedFuelTank;
            set
            {
                if (SelectedFuelTank != null)
                {
                    SelectedFuelTank?.UnSelect();
                }

                if (SetProperty(ref _selectedFuelTank, value))
                {
                    ActionInViewModel(() =>
                    {
                        SelectedFuelTank.Select();
                    });

                    RaisePropertyChanged(nameof(HitTankItemName));
                }
            }
        }

        private FuelTankMesh _selectedFuelTank = null;

        public FuelTankMesh CurrentFuelTankInHeatMap
        {
            get => _currentFuelTankInHeatMap;
            set
            {
                if (SetProperty(ref _currentFuelTankInHeatMap, value))
                {
                    RaisePropertyChanged(nameof(HitTankItemName));
                }
            }
        }
        private FuelTankMesh _currentFuelTankInHeatMap = null;

        public FuelTankMesh HitFuelTank
        {
            get => _hitFuelTank;
            set
            {
                if (SetProperty(ref _hitFuelTank, value))
                {
                    CreateHeatMap();
                    RaisePropertyChanged(nameof(HitTankItemName));
                }
            }
        }
        private FuelTankMesh _hitFuelTank = null;

        public string HitSensorItemName
        {
            get
            {
                if (HitSensor != null)
                {
                    return $"Объект под курсором - {HitSensor.Name}";
                }

                return string.Empty;
            }
        }

        public string HitTankItemName
        {
            get
            {
                if (HitFuelTank != null)
                {
                    return $"Объект под курсором - {HitFuelTank.Name}";
                }

                return string.Empty;
            }
        }

        #region Camera

        private bool _zoomAroundMouseDownPoint = true;

        public bool ZoomAroundMouseDownPoint
        {
            get => _zoomAroundMouseDownPoint;
            set => SetProperty(ref _zoomAroundMouseDownPoint, value);
        }


        private bool _rotateAroundMouseDownPoint = true;

        public bool RotateAroundMouseDownPoint
        {
            get => _rotateAroundMouseDownPoint;
            set => SetProperty(ref _rotateAroundMouseDownPoint, value);
        }

        private bool _isChangeFieldOfViewEnabled = true;

        public bool IsChangeFieldOfViewEnabled
        {
            get => _isChangeFieldOfViewEnabled;
            set => SetProperty(ref _isChangeFieldOfViewEnabled, value);
        }
        #endregion

        private void InitializeSliders()
        {
            _minimumPitch = _calculationModel.CurrentBranch.AnglesModel.MinPitch;
            _maximumPitch = _calculationModel.CurrentBranch.AnglesModel.MaxPitch;
            _minimumRoll  = _calculationModel.CurrentBranch.AnglesModel.MinRoll;
            _maximumRoll  = _calculationModel.CurrentBranch.AnglesModel.MaxRoll;
        }

        private void InitializeCommand()
        {
            OnCanvasViewLoaded = new DelegateCommand<object>(CanvasViewLoaded);
            OnCanvasSizeChanged = new DelegateCommand(CanvasSizeChanged);
            OnErrorsTabLoaded = new DelegateCommand<object>(ErrorsLoaded);
            OnCapacityTabLoaded = new DelegateCommand<object>(CapacityLoaded);
            UCOnLoaded = new DelegateCommand<object>(ComputeTypeControlLoaded);

            SwitchTankVisibilityCommand = new DelegateCommand<string>((id) =>
            {
                var tank = FuelTanks.FirstOrDefault(x => x.Id == id);

                if (tank != null)
                {
                    ActionInViewModel(() =>
                    {
                        DrawGraphics();
                        SetTankPoints();

                        tank.UnSelect();
                    });
                }
            });

            NavigateOnTankCommand = new DelegateCommand<string>((id) =>
            {
                var tank = FuelTanks.FirstOrDefault(x => x.Id == id);

                if (tank != null)
                {
                    ActionInViewModel(() =>
                    {
                        var point = tank.Geometry.BoundingSphere.Center;

                        Viewport3Dx.LookAt(new Point3D(point.X, point.Y, point.Z), tank.Geometry.BoundingSphere.Radius * 2, 500);
                    });
                }
            });

            RemoveSelectedSensorCommand = new DelegateCommand(() =>
            {
                if (SelectedSensor != null)
                {
                    FuelSensors.Remove(SelectedSensor);

                    var tankDTO = CurrentConfiguration.FuelTanks
                        .OfType<FuelTankModelDTO>()
                        .FirstOrDefault(x => x.Sensors
                        .Where(x => x.Id == SelectedSensor.Id)
                        .Any()
                        );

                    if (tankDTO != null)
                    {
                        var sensorDTO = tankDTO.Sensors
                            .FirstOrDefault(x => x.Id == SelectedSensor.Id);

                        if (sensorDTO != null)
                        {
                            _computationController.DeleteSensor(_calculationModel, sensorDTO);
                            (tankDTO.Sensors as ObservableCollection<SensorModelDTO>).Remove(sensorDTO);
                            DrawGraphics();
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Метод инициализации графиков
        /// </summary>
        private void InitializeGraphics()
        {
            _calculationModel = _calculationController.GetCalculationModel();

            // Вызываем инициализатор графопостроителя
            _computationController.GraphicsInitialize(_calculationModel, GeometryList);

            SetTankPoints();
        }


        private void OnViewerModeChanged(ViewerWorkingModes newMode)
        {
            ViewerWorkingMode = newMode;

            var points = FuelTanks
                .ToList()
                .Select(tank =>
                tank.Geometry.BoundingSphere.Center
                );

            var center = new Vector3D(
                points.Average(point => point.X),
                points.Average(point => point.Y),
                points.Average(point => point.Z)
                );

            var radius = FuelTanks
                .ToList()
                .Select(tank =>
                tank.Geometry.BoundingSphere.Radius)
                .Sum();

            var minZ = points.Min(point => point.Z);
            var maxZ = points.Max(point => point.Z);

            switch (ViewerWorkingMode)
            {
                case ViewerWorkingModes.Manipulating:

                    ActionInViewModel(() =>
                    {
                        Camera =
                            new HelixToolkit.Wpf.SharpDX.OrthographicCamera
                            {
                                Position = RenderingConstants.ManipulationModeCameraPosition,
                                NearPlaneDistance = RenderingConstants.DefaultNearPlaneDistance,
                                FarPlaneDistance = RenderingConstants.DefaultFarPlaneDistance,
                                UpDirection = RenderingConstants.ManipulationModeCameraUpDirection,
                                LookDirection = RenderingConstants.ManipulationModeCameraLookDirection
                            };

                        (Camera as HelixToolkit.Wpf.SharpDX.OrthographicCamera).AnimateWidth(
                            (maxZ - minZ) * 1.3,
                            RenderingConstants.DefaultCameraAnimationTime
                            );

                        Viewport3Dx.LookAt(
                            new Point3D(
                                center.X,
                                center.Y,
                                center.Z
                                ),
                            radius,
                            RenderingConstants.DefaultCameraAnimationTime
                            );
                    });

                    FuelTanks
                        .ToList()
                        .ForEach(tank =>
                        tank.UnSelect());

                    break;
                case ViewerWorkingModes.View:

                    ActionInViewModel(() =>
                    {
                        Camera =
                            new PerspectiveCamera
                            {
                                Position = RenderingConstants.ViewModeCameraPosition,
                                NearPlaneDistance = RenderingConstants.DefaultNearPlaneDistance,
                                FarPlaneDistance = RenderingConstants.DefaultFarPlaneDistance,
                                UpDirection = RenderingConstants.ViewModeCameraUpDirection,
                                LookDirection = RenderingConstants.ViewModeCameraLookDirection
                            };

                        Viewport3Dx.LookAt(
                            new Point3D(
                                center.X,
                                center.Y,
                                center.Z
                                ),
                            radius,
                            RenderingConstants.DefaultCameraAnimationTime
                            );
                    });

                    FuelSensors
                        .ToList()
                        .ForEach(tank =>
                        tank.UnSelect());

                    break;
            }
        }

        public void Viewport3DxOnLoaded(object sender, RoutedEventArgs e)
        {
            Viewport3Dx = (Viewport3DX)sender;
        }

        /// <summary>
        /// Метод после загрузки окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            Window = (Window)sender;
        }


        private Point3D _holdCameraPosition;

        public void CameraChanged(object sender, RoutedEventArgs e)
        {
            _holdCameraPosition = Camera.Position;
        }

        public void OnMouseMouseMouseDown3DHandler(object sender, MouseDown3DEventArgs e)
        {
            if (ViewerWorkingMode == ViewerWorkingModes.Manipulating)
            {
                if (e.OriginalInputEventArgs is MouseButtonEventArgs mouseButtonArgs)  
                {
                    if(mouseButtonArgs.ChangedButton == MouseButton.Left)
                    {
                        leftMouseButtonPressed = true;

                        if (mouseButtonArgs.ClickCount == 2)
                        {
                            if (HitFuelTank != null)
                            {
                                if (HitFuelTank.Geometry is GeometryInfoMesh3D mesh)
                                {
                                    if(_hitTestService.TryFindSensorPoints(e.HitTestResult.PointHit, mesh, out var results))
                                    {
                                        if(results.Any())
                                        {
                                            CreateNewSensor(
                                                results.Select(x => x.PointHit)
                                                    .OrderByDescending(x => x.Y)
                                                    .ToList(),
                                                    "Новый датчик",
                                                    mesh.Id
                                                    );
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var viewport3Dx = (Viewport3DX)sender;
                            var hits = viewport3Dx.FindHits(e.Position);

                            if (!hits.Any())
                            {
                                return;
                            }

                            List<HelixToolkit.SharpDX.Core.Geometry3D> findHits = hits
                                .Select(x => x.Geometry)
                                .Where(x => (x as GeometryInfoMesh3D) != null)
                                .ToList();

                            foreach (var hit in findHits)
                            {
                                if(hit is GeometryInfoMesh3D geometry)
                                {
                                    switch (geometry.MeshType)
                                    {
                                        case MeshType.FuelSensor:

                                            SelectedSensor = FuelSensors
                                                .FirstOrDefault(x => x.Id == geometry.Id);

                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Метод создания нового датчика
        /// </summary>
        /// <param name="points"></param>
        /// <param name="name"></param>
        /// <param name="tankId"></param>
        private FuelSensorMesh CreateNewSensor(List<Vector3> points, string name, string tankId)
        {
            var fuelSensorMesh = new FuelSensorMesh(
                Guid.NewGuid().ToString(),
                name,
                PhongMaterials.Green,
                points.First(),
                points.Last(),
                _indentService,
                tankId
                );

            fuelSensorMesh.SensorChangePosition += FuelSensorMesh_SensorChangePosition;

            SensorModelDTO sensorDTO = new()
            {
                Name = name,
                TankId = tankId,
                ProjectId = CurrentConfiguration.ProjectId,
                Id = fuelSensorMesh.Id,
                UpPointIndent = fuelSensorMesh.UpPointIndent,
                DownPointIndent = fuelSensorMesh.DownPointIndent,
                UpPoint = fuelSensorMesh.SensorPosition.UpPoint.ToVector3(),
                DownPoint = fuelSensorMesh.SensorPosition.DownPoint.ToVector3(),
                InTankUpPoint = fuelSensorMesh.SensorPosition.InTankUpPoint.ToVector3(),
                InTankDownPoint = fuelSensorMesh.SensorPosition.InTankDownPoint.ToVector3(),
                Length = fuelSensorMesh.Length,
                LinearCapacity = fuelSensorMesh.LinearCapacity
            };

            var tankDto = CurrentConfiguration.FuelTanks
                .OfType<FuelTankModelDTO>()
                .FirstOrDefault(x => x.Id == tankId);

            (tankDto.Sensors as ObservableCollection<SensorModelDTO>).Add(sensorDTO);
            FuelSensors.Add(fuelSensorMesh);



            // Вот тут по идее, можно обработать "событие" создания датчика, но можно сделать и отдельное событие и стучать в него из конструктора, но мне кажется это излишне

            #region Готов ЭТО перенести в событие 
            
            _computationController.AddSensor(_calculationModel, tankId, sensorDTO);

            DrawGraphics();
            #endregion

            return fuelSensorMesh;
        }

        /// <summary>
        /// Метод обработчика перемещения датчика
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newPosition"></param>
        private void FuelSensorMesh_SensorChangePosition(string id, SensorPosition newPosition)
        {
            var tankDTO = CurrentConfiguration.Branches
                .FirstOrDefault(x => x.Type == BranchType.Working).FuelTanks
                .FirstOrDefault(x => x.Id == SelectedSensor.FuelTankId);

            if (tankDTO != null)
            {
                var sensorDTO = tankDTO.Sensors
                    .FirstOrDefault(x => x.Id == id);

                if (sensorDTO != null)
                {
                    sensorDTO.UpPoint = newPosition.UpPoint.ToVector3();
                    sensorDTO.DownPoint = newPosition.DownPoint.ToVector3();

                    sensorDTO.InTankUpPoint = newPosition.InTankUpPoint.ToVector3();
                    sensorDTO.InTankDownPoint = newPosition.InTankDownPoint.ToVector3();

                    sensorDTO.UpPointIndent = newPosition.UpPointIndent;
                    sensorDTO.DownPointIndent = newPosition.DownPointIndent;
                }
            }

            // Тут можно обрабатывать передвижение датчика
            // в SensorPosition есть точки датчика и точки крепления, если что то нужно еще напишите, ну кроме того, что высчитывается, например длина

            // SelectedSensor - выделенный датчик
            // HitSensor - датчик над которым курсор мыши
            TankModel tank = _calculationModel.SelectedTanks.FirstOrDefault(x => x.Id == SelectedSensor.FuelTankId);
            if(tank != null)
            {
                var _sensor = tank.Sensors.Find(x => x.SensorId == id);
                if (_sensor != null)
                {
                    Point3D upPoint = newPosition.UpPoint.ToPoint3DAndRound();
                    Point3D downPoint = newPosition.DownPoint.ToPoint3DAndRound();
                    _computationController.MoveSensor(_calculationModel, _sensor, upPoint, downPoint);
                    UpdateGraphics();
                }
            }
        }

        private bool leftMouseButtonPressed = false;

        public void OnMouseUpHandler(object sender, MouseUp3DEventArgs e)
        {
            if (e.OriginalInputEventArgs is MouseButtonEventArgs { ChangedButton: MouseButton.Left })
            {
                leftMouseButtonPressed = false;

                if (e.HitTestResult == null)
                {
                    FuelTanks.ToList().ForEach(tank => tank.UnSelect());
                    FuelSensors.ToList().ForEach(sensor => sensor.UnSelect());
                    //клик в пустоту
                    return;
                }

                var viewport3Dx = (Viewport3DX)sender;
                var hits = viewport3Dx.FindHits(e.Position);

                if (!hits.Any())
                {
                    return;
                }

                List<HelixToolkit.SharpDX.Core.Geometry3D> findHits = hits   
                    .Select(x => x.Geometry)
                    .Where(x => (x as GeometryInfoMesh3D) != null)
                    .ToList();

                switch (ViewerWorkingMode)
                {
                    case ViewerWorkingModes.View:
                    {
                        foreach (var hit in findHits)
                        {
                            if (hit is GeometryInfoMesh3D info)
                            {
                                var tank = FuelTanks.FirstOrDefault(tank => tank.Id == info.Id);

                                if(tank != null)
                                {
                                    SelectedFuelTank = tank;
                                    break;
                                }
                            }
                        }

                        break;
                    }
                    case ViewerWorkingModes.Manipulating:
                    {
                        foreach (var hit in findHits)
                        {
                            if (hit is GeometryInfoMesh3D info)
                            {
                                var selectedSensor = FuelSensors.FirstOrDefault(sensor => sensor.Id == info.Id);

                                if (selectedSensor != null)
                                {
                                    ActionInViewModel(() =>
                                    {
                                        FuelSensors
                                            .Except(new List<FuelSensorMesh>() { selectedSensor })
                                            .ToList()
                                            .ForEach(sensor =>
                                            {
                                                sensor.UnSelect();
                                            });

                                        selectedSensor.Select();
                                    });
                                    break;
                                }
                            }
                        }

                        break;
                    }    
                }
            }
        }

        public void OnMouseMove3DHandler(object sender, MouseMove3DEventArgs e)
        {
            if (Viewport3Dx.CursorOnElementPosition != null)
            {
                MousePosition = Viewport3Dx.CursorOnElementPosition;
            }
            else
            {
                MousePosition = Viewport3Dx.CursorPosition;
            }

            var viewport3Dx = (Viewport3DX)sender;
            var hits = viewport3Dx.FindHits(e.Position);

            if (!hits.Any())
            {
                HitFuelTank = null;
                HitSensor = null;
                return;
            }

            bool sensorAtCursor = false;
            bool tankAtCursor = false;

            HelixToolkit.SharpDX.Core.HitTestResult hitTestResult = null;
            foreach (var hitTestItem in hits.OrderBy(or => or.Distance))
            {
                if (hitTestItem.Geometry is not GeometryInfoMesh3D meshGeometry)
                    continue;

                if (hitTestResult == null)
                {
                    hitTestResult = hitTestItem;
                    continue;
                }

                switch (meshGeometry.MeshType)
                {
                    case MeshType.FuelTank:
                    {
                        HitFuelTank = FuelTanks.FirstOrDefault(tank => tank.Id == meshGeometry.Id);

                        if (HitFuelTank != null)
                        {
                            tankAtCursor = true;
                        }

                        break;
                    }
                    case MeshType.FuelSensor:
                    {
                        HitSensor = FuelSensors.FirstOrDefault(sensor => sensor.Id == meshGeometry.Id);

                        if (HitSensor != null)
                        {
                            sensorAtCursor = true;
                        }

                        break;
                    }
                }

                if (!tankAtCursor)
                {
                    HitFuelTank = null;
                }

                if (!sensorAtCursor)
                {
                    HitSensor = null;
                }
            }

            if(leftMouseButtonPressed)
            {
                if (SelectedSensor != null)
                {
                    if (HitFuelTank != null)
                    {
                        if (MousePosition != null && MousePosition is Point3D point)
                        {
                            var target = new Vector3() 
                            {
                                X = Convert.ToSingle(point.X),
                                Y = Convert.ToSingle(point.Y + 50),
                                Z = Convert.ToSingle(point.Z)
                            };

                            if (HitFuelTank.Geometry.TryFindHits(target, RenderingConstants.HitTestDownVector, out var result))
                            {
                                var points = result
                                    .Select(x => x.PointHit)
                                    .OrderByDescending(x => x.Y)
                                    .ToList();

                                if (points.Any() && points.Count == 2)
                                {
                                    SelectedSensor.TrySetNewPosition(points[0], points[1]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ActionInViewModel(
            Action action, 
            DispatcherPriority priority = DispatcherPriority.Normal
            )
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

        public async Task ActionInViewModelAsync(
            Action action, 
            DispatcherPriority priority = DispatcherPriority.Normal
            )
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

        public void Closed()
        {
            FuelSensors.ToList().ForEach(sensor => 
            {
                sensor.SensorChangePosition -= FuelSensorMesh_SensorChangePosition;
            });

            _switchModeToken.Dispose();
            Viewport3Dx?.Dispose();

            UpPlot.Plot.Clear();
            DownPlot.Plot.Clear();

        }


        

        public WpfPlot UpPlot { get; set; }
        public WpfPlot DownPlot { get; set; }

        public void UpHeatMapLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if(sender is WpfPlot plot)
            {
                UpPlot = plot;
                _plotterService.ConfigurePlotter(
                    UpPlot, 
                    ScottPlot.Control.QualityMode.Low, 
                    false, 
                    false, 
                    false, 
                    false
                    );
            }
        }

        public void DownHeatMapLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is WpfPlot plot)
            {
                DownPlot = plot;
                _plotterService.ConfigurePlotter(
                    DownPlot, 
                    ScottPlot.Control.QualityMode.Low, 
                    false, 
                    false, 
                    false, 
                    false
                    );
            }
        }

        private void CreateHeatMap()
        {
            if (HitFuelTank != null)
            {
                if (CurrentFuelTankInHeatMap != HitFuelTank)
                {
                    if (SelectedSensor == null)
                    {
                        SetHeatMap();
                    }
                    else
                    {
                        if(CurrentFuelTankInHeatMap != null)
                        {
                            if (CurrentFuelTankInHeatMap.Id != SelectedSensor.FuelTankId)
                            {
                                if (SelectedSensor.FuelTankId == HitFuelTank.Id)
                                {
                                    SetHeatMap();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetHeatMap()
        {
            if (!HitFuelTank.UpZones.Any() && !HitFuelTank.DownZones.Any())
            {
                _heatMapService.CreateHeatMap(HitFuelTank);
            }

            ActionInViewModel(() =>
            {
                ShowZone(UpPlot, HitFuelTank.UpZones);

                ShowZone(DownPlot, HitFuelTank.DownZones);

                if (HitFuelTank.UpZones.Any() && HitFuelTank.DownZones.Any())
                {
                    CurrentFuelTankInHeatMap = HitFuelTank;
                }

                HeatMapRender();
            });
        }

        private static void ShowZone(WpfPlot plot, List<HeatMapZoneModel> zones)
        {
            plot.Plot.Clear();

            foreach (var zone in zones)
            {
                foreach (var point in zone.Points)
                {
                    var p = (Vector3)point;

                    plot.Plot.AddPoint(
                        -p.Z, 
                        -p.X, 
                        System.Drawing.Color.FromArgb(
                            zone.ZoneColor.A, 
                            zone.ZoneColor.R, 
                            zone.ZoneColor.G, 
                            zone.ZoneColor.B
                            ),
                        20, 
                        MarkerShape.filledSquare
                        );
                }
            }
        }

        public void HeatMapRender()
        {
            ActionInViewModel(() =>
            {
                UpPlot.RenderRequest(RenderType.LowQualityThenHighQualityDelayed);
                DownPlot.RenderRequest(RenderType.LowQualityThenHighQualityDelayed);
            });
        }

        

        #region Это перенести потом в настройки
        private static FuelTankTabControlStates _fuelTankTabControlStates = FuelTankTabControlStates.ErrorsTab;
        public static FuelTankTabControlStates FuelTankTabControlStates
        {
            get => _fuelTankTabControlStates;
            set
            {
                if (_fuelTankTabControlStates == value) return;
                _fuelTankTabControlStates = value;
            }
        }
        #endregion


        private int _selectedTab = (int)FuelTankTabControlStates;
        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab == value) return;
                ComputeType = ComputeTypeEnum.ErrorsUsual;
                _selectedTab = value;
                FuelTankTabControlStates = (FuelTankTabControlStates)value;

                RaisePropertyChanged(nameof(SelectedTab));
                DrawGraphics();
            }
        }

        private void DrawGraphics()
        {
            switch (FuelTankTabControlStates)
            {
                case FuelTankTabControlStates.ErrorsTab:
                    try
                    {
                        ActionInViewModel(() =>
                        {
                            _plotterController.SetErrorsCurves(_calculationModel, _errorsPlot, _calculationModel.BranchTanks, PitchValue, RollValue);
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Ошибка генерации погрешности измерений: {ex}");
                    }
                    break;
                case FuelTankTabControlStates.CalibrationTab:
                    PlotTaringCurves(); // Отрисовываем кривые с датчиками - подумать - может, датчики отдельно отрисовывать
                    break;
                case FuelTankTabControlStates.CapacityTab:
                    try
                    {
                        ActionInViewModel(() =>
                        {
                            _plotterController.SetCapacityCurves(_calculationModel, _capacityPlot, PitchValue, RollValue);
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Ошибка генерации кривой погонной емкости: {ex}");
                    }
                    break;
            }
        }

        private void UpdateGraphics()
        {
            switch (FuelTankTabControlStates)
            {
                case FuelTankTabControlStates.ErrorsTab:
                    try
                    {
                        ActionInViewModel(() =>
                        {
                            _plotterController.UpdateErrorsCurves(_calculationModel, _errorsPlot, _calculationModel.BranchTanks, PitchValue, RollValue);
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Ошибка генерации погрешности измерений: {ex}");
                    }
                    break;
                case FuelTankTabControlStates.CalibrationTab:
                    PlotTaringCurves(); // Отрисовываем кривые с датчиками - подумать - может, датчики отдельно отрисовывать
                    break;
                case FuelTankTabControlStates.CapacityTab:
                    try
                    {
                        ActionInViewModel(() =>
                        {
                            _plotterController.UpdateCapacityCurves(_calculationModel, _capacityPlot, PitchValue, RollValue);
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Ошибка генерации кривой погонной емкости: {ex}");
                    }
                    break;
            }
        }


        public void ComputeTypeControlLoaded(object element)
        {
            ComputeTypeControl = element as UserControl;
        }

        /// <summary>
        /// Отрисовка МПИ
        /// </summary>
        private void PlotErrorCurves()
        {
            try
            {
                ActionInViewModel(() =>
                {
                    _plotterController.SetErrorsCurves(_calculationModel, _errorsPlot, _calculationModel.BranchTanks, PitchValue, RollValue);
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка генерации погрешности измерений: {ex}");
            }
        }

        /// <summary>
        /// Отрисовка тарировочной кривой
        /// </summary>
        private void PlotTaringCurves()
        {
            try
            {
                ActionInViewModel(() =>
                {
                    if (_canvas != null) _plotterController.FillTaringCanvas(_canvas, _calculationModel, true, null, PitchValue, RollValue);
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка генерации тарировочной кривой: {ex}");
            }
        }

        /// <summary>
        /// Отрисовка кривой погонной емкости
        /// </summary>
        private void PlotCapacityCurves()
        {
            if (FuelTankTabControlStates == FuelTankTabControlStates.CapacityTab)
            {
                try
                {
                    ActionInViewModel(() =>
                    {
                        _plotterController.SetCapacityCurves(_calculationModel, _capacityPlot, PitchValue, RollValue);
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"Ошибка генерации кривой погонной емкости: {ex}");
                }
            }
        }

        /// <summary>
        /// Отображение МПИ
        /// </summary>
        /// <param name="element"></param>
        private void ErrorsLoaded(object element)
        {
            _errorsPlot = element as WpfPlot;
            if (FuelTankTabControlStates == FuelTankTabControlStates.ErrorsTab)
            {
                _errorsPlot.MouseMove += ErrorsPlot_MouseMove;
                _errorsPlot.MouseLeave += ErrorsPlot_MouseLeave;
                _errorsPlot.MouseEnter += ErrorsPlot_MouseEnter;
                PlotErrorCurves();
            }
        }

        /// <summary>
        /// Мышь попала в область графика МПИ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorsPlot_MouseEnter(object sender, MouseEventArgs e)
        {
            _lastHighlightedIndex = -1;
            ErrorsPlot_MouseMove(sender, e);
        }

        /// <summary>
        /// Метод удаления шариков на кривой МПИ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorsPlot_MouseLeave(object sender, MouseEventArgs e)
        {
            _errorsPlot.Plot.Remove(CurrentPoint); // Удаляем выделение! Всего делов-то!!!
            _dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        _errorsPlot.RenderRequest(RenderType.LowQualityThenHighQualityDelayed);
                    }));
        }

        /// <summary>
        /// Метод формирования шариков на точках перегиба кривой МПИ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorsPlot_MouseMove(object sender, MouseEventArgs e)
        {
            //if (CurrentErrorSerie == null) return; // Если графика нет, то выходим

            // Получаем координаты курсора мыши
            (double mouseCoordinateX, double mouseCoordinateY) = _errorsPlot.GetMouseCoordinates();

            CurrentVolume = mouseCoordinateX;
            CurrentError = mouseCoordinateY;

            // Высчитываем ближайшую к курсору точку графика - точки кривой сформированы в другом месте
            (double pointX, double pointY, int pointIndex) = _plotterController.GetErrorNearest(mouseCoordinateX, mouseCoordinateY);


            if (CurrentPoint == null)
            {
                _currentX[0] = pointX;
                _currentY[0] = pointY;
                CurrentPoint = _errorsPlot.Plot.AddBubblePlot(
                    _currentX,
                    _currentY,
                    radius: 6,
                    System.Drawing.Color.Gold,
                    edgeColor: System.Drawing.Color.Chocolate,
                    edgeWidth: 2);
            }

            if (_lastHighlightedIndex != pointIndex)
            {
                _lastHighlightedIndex = pointIndex;
                (NearestVolume, NearestError) = _plotterController.GetErrorSeriePoints(pointIndex);
                _errorsPlot.Plot.Remove(CurrentPoint);
                _currentX[0] = pointX;
                _currentY[0] = pointY;
                CurrentPoint = _errorsPlot.Plot.AddBubblePlot(
                                                _currentX,
                                                _currentY,
                                                radius: 6,
                                                System.Drawing.Color.Gold,
                                                edgeColor: System.Drawing.Color.Chocolate,
                                                edgeWidth: 2);

                _dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        _errorsPlot.RenderRequest(RenderType.LowQualityThenHighQualityDelayed);
                    }));
            }
        }

        /// <summary>
        /// Отображение погонной емкости
        /// </summary>
        /// <param name="element"></param>
        private void CapacityLoaded(object element)
        {
            _capacityPlot = element as WpfPlot;
            if (FuelTankTabControlStates == FuelTankTabControlStates.CapacityTab)
            {
                _capacityPlot.MouseMove += CapacityPlot_MouseMove;
                _capacityPlot.MouseLeave += CapacityPlot_MouseLeave;
                _capacityPlot.MouseEnter += CapacityPlot_MouseEnter;
                PlotCapacityCurves();
            }
        }

        /// <summary>
        /// Мышь попала в область графика погонной емкости
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CapacityPlot_MouseEnter(object sender, MouseEventArgs e)
        {
            _lastCapacityHighlightedIndex = -1;
            CapacityPlot_MouseMove(sender, e);
        }

        /// <summary>
        /// Метод удаления шариков на кривой погонной емкости
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CapacityPlot_MouseLeave(object sender, MouseEventArgs e)
        {
            _capacityPlot.Plot.Remove(CapacityPoint);
            _dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        _capacityPlot.RenderRequest(RenderType.LowQualityThenHighQualityDelayed);
                    }));
        }

        /// <summary>
        /// Метод формирования шариков на точках перегиба графика погонной емкости
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CapacityPlot_MouseMove(object sender, MouseEventArgs e)
        {
            //if (CurrentCapacitySerie == null) return; // Если графика нет, то выходим

            // Получаем координаты курсора мыши
            (double mouseCoordinateX, double mouseCoordinateY) = _capacityPlot.GetMouseCoordinates();

            CapacityVolume = mouseCoordinateX;
            CurrentCapacity = mouseCoordinateY;

            //Высчитываем ближайшую к курсору точку графика -точки кривой сформированы в другом месте
            (double pointX, double pointY, int pointIndex) = _plotterController.GetCapacityNearest(mouseCoordinateX, mouseCoordinateY);

            if (CapacityPoint == null)
            {
                _currentX[0] = pointX;
                _currentY[0] = pointY;
                CapacityPoint = _capacityPlot.Plot.AddBubblePlot(_currentX, _currentY,
                                                                 radius: 6, System.Drawing.Color.Gold,
                                                                 edgeColor: System.Drawing.Color.Chocolate, edgeWidth: 2);
            }

            if (_lastCapacityHighlightedIndex != pointIndex)
            {
                _lastCapacityHighlightedIndex = pointIndex;
                (NearestCapacityVolume, NearestCapacity) = _plotterController.GetErrorSeriePoints(pointIndex);
                _capacityPlot.Plot.Remove(CapacityPoint);
                _currentX[0] = pointX;
                _currentY[0] = pointY;
                CapacityPoint = _capacityPlot.Plot.AddBubblePlot(_currentX, _currentY,
                                                                 radius: 6, System.Drawing.Color.Gold,
                                                                 edgeColor: System.Drawing.Color.Chocolate, edgeWidth: 2);

                _dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        _capacityPlot.RenderRequest(RenderType.LowQualityThenHighQualityDelayed);
                    }));
            }
        }

        /// <summary>
        /// Загрузка графика тарировочной кривой
        /// </summary>
        /// <param name="element"></param>
        private void CanvasViewLoaded(object element)
        {
            _canvas = element as Canvas;
            if (FuelTankTabControlStates == FuelTankTabControlStates.CalibrationTab)
            {
                PlotTaringCurves();
            }
        }

        /// <summary>
        /// Перерисовка тарировочного графика при изменении размера окна (в частности - канвы)
        /// </summary>
        private void CanvasSizeChanged()
        {
            if (FuelTankTabControlStates == FuelTankTabControlStates.CalibrationTab)
            {
                PlotTaringCurves();
            }
        }





        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="geometryRepository"></param>
        /// <param name="viewportElementService"></param>
        /// <param name="viewportElementManipulationService"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="regionManager"></param>
        /// <param name="calculationController"></param>
        /// <param name="plotterController"></param>
        /// <param name="container"></param>
        /// <param name="indentService"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Viewer3DViewModel(ILogger logger,
                                 IGeometryRepository geometryRepository,
                                 IViewportElementService viewportElementService,
                                 IViewportElementManipulationService viewportElementManipulationService,
                                 IEventAggregator eventAggregator,
                                 IRegionManager regionManager,
                                 ICalculationController calculationController,
                                 IPlotterController plotterController,
                                 ICapacityController capacityController,
                                 IContainer container,
                                 IComputationController computationController,
                                 IIndentService indentService,
                                 IHitTestService hitTestService,
                                 IRepository repository,
                                 IHeatMapService heatMapService,
                                 IMaterialService materialService,
                                 IPlotterSettingsService plotterService)
        {
            _logger                             = logger ?? throw new ArgumentNullException(nameof(logger));
            _geometryRepository                 = geometryRepository ?? throw new ArgumentNullException(nameof(geometryRepository));
            _viewportElementService             = viewportElementService ?? throw new ArgumentNullException(nameof(viewportElementService));
            _viewportElementManipulationService = viewportElementManipulationService ?? throw new ArgumentNullException(nameof(viewportElementManipulationService));
            _eventAggregator                    = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _indentService                      = indentService ?? throw new ArgumentNullException(nameof(indentService));
            _calculationController              = calculationController ?? throw new ArgumentNullException(nameof(calculationController));
            _plotterController                  = plotterController ?? throw new ArgumentNullException(nameof(plotterController));
            _capacityController                 = capacityController ?? throw new ArgumentNullException(nameof(capacityController));
            _computationController              = computationController ?? throw new ArgumentNullException(nameof(computationController));
            _hitTestService                     = hitTestService ?? throw new ArgumentNullException(nameof(hitTestService));
            _repository                         = repository ?? throw new ArgumentNullException(nameof(repository));
            _heatMapService                     = heatMapService ?? throw new ArgumentNullException(nameof(heatMapService));
            _materialService                    = materialService ?? throw new ArgumentNullException(nameof(materialService));
            _plotterService                     = plotterService ?? throw new ArgumentNullException(nameof(plotterService));
            _regionManager                      = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _container                          = container ?? throw new ArgumentNullException(nameof(container));

            _switchModeToken = _eventAggregator.GetEvent<SwitchViewerModeEvent>().Subscribe(OnViewerModeChanged);

            InitializeCommand();

            GeometryList = new ObservableCollection<FuelTankGeometryModel>(_geometryRepository.GetAllGeometry());
            InitializeGraphics();

            CurrentConfiguration = _repository
                .GetCurrentProject()
                .Configurations
                .FirstOrDefault(x =>
                x.Type == ConfigurationType.Working
                );

            var instanceThread = Thread.CurrentThread;
            Task.Factory.StartNew(() =>
            {
                while (Dispatcher.FromThread(instanceThread) == null) Thread.Sleep(10);

                _dispatcher = Dispatcher.FromThread(instanceThread);

                ActionInViewModel(() =>
                {
                    EffectsManager = new DefaultEffectsManager();

                    Camera = new PerspectiveCamera
                    {
                        Position          = RenderingConstants.ViewModeCameraPosition,
                        NearPlaneDistance = RenderingConstants.DefaultNearPlaneDistance,
                        FarPlaneDistance  = RenderingConstants.DefaultFarPlaneDistance,
                        UpDirection       = RenderingConstants.ViewModeCameraUpDirection,
                        LookDirection     = RenderingConstants.ViewModeCameraLookDirection
                    };

                    CoordinateLines.AddRange(_viewportElementService.CreateViewportLines());

                    GridLines.Add(_viewportElementService.CreateViewportGrid());

                    var material = _materialService.CreateMaterial(Colors.Silver, 0.3f);
                    var insideMaterial = _materialService.CreateMaterial(Colors.Black, 0.3f);

                    foreach (var tankGeometry in GeometryList) // Наверно тут сделать получение по ид конфигурации, а не вообще все
                    {
                        switch (tankGeometry.Type)
                        {
                            case TankGeometryType.TankGeometry:
                                FuelTanks.Add(new FuelTankMesh(
                                    material,
                                    material,
                                    tankGeometry
                                    ));

                                break;

                            case TankGeometryType.InsideModelGeometry:
                                InsideModelFuelTanks.Add(new InsideModelFuelTankMesh(
                                    insideMaterial,
                                    insideMaterial,
                                    tankGeometry
                                    ));

                                break;
                        }

                        var tankDTO = CurrentConfiguration.FuelTanks
                            .FirstOrDefault(x => x.Id == tankGeometry.Id);

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

                                fuelSensorMesh.SensorChangePosition += FuelSensorMesh_SensorChangePosition;
                                FuelSensors.Add(fuelSensorMesh);
                            });
                        }
                    }

                    var points = FuelTanks
                        .ToList()
                        .Select(tank =>
                        tank.Geometry.BoundingSphere.Center
                        );

                    var center = new Vector3D(
                        points.Average(point => point.X),
                        points.Average(point => point.Y),
                        points.Average(point => point.Z)
                        );

                    var radius = FuelTanks
                        .ToList()
                        .Select(tank =>
                        tank.Geometry.BoundingSphere.Radius)
                        .Sum();

                    Viewport3Dx.LookAt(
                        new Point3D(
                            center.X,
                            center.Y,
                            center.Z
                            ),
                        radius,
                        RenderingConstants.DefaultCameraAnimationTime
                        );
                });
            });

            InitializeSliders();
        }
    }
}
