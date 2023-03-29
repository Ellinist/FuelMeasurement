using AutoMapper;
using FuelMeasurement.Client.Models;
using FuelMeasurement.Client.Services.Interfaces;
using FuelMeasurement.Client.UIModule.Models;
using FuelMeasurement.Client.UIModule.Services;
using FuelMeasurement.Client.UIModule.Services.Interfaces;
using FuelMeasurement.Data.Repositories.Repositories.Interfaces;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using NLog;
using Prism.Events;
using Prism.Mvvm;
using SharpDX;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace FuelMeasurement.Client.ViewModels
{
    public class SelectedItemWrapperViewModel : BindableBase, IDisposable, IActionInViewModel
    {
        private bool _disposed = false;

        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private Dispatcher Dispatcher;
        private readonly IGeometryRepository _geometryRepository;
        private readonly ISelectionManager _selectionManager;
        private readonly IViewportElementService _viewportElementService;
        private readonly IMaterialService _materialService;
        private readonly IIndentService _indentService;

        private ModelBase _currentItem;
        public ModelBase CurrentItem
        {
            get => _currentItem;
            set => SetProperty(ref _currentItem, value);
        }

        public Viewport3DX Viewport3Dx
        { get; set; }

        public HelixToolkit.Wpf.SharpDX.PerspectiveCamera Camera
        {
            get => _camera;
            protected set => SetProperty(ref _camera, value);
        }
        private HelixToolkit.Wpf.SharpDX.PerspectiveCamera _camera;

        public IEffectsManager EffectsManager
        {
            get => _effectsManager;
            protected set => SetProperty(ref _effectsManager, value);
        }
        private IEffectsManager _effectsManager;

        public Window Window { get; set; }

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
        private ObservableCollection<FuelTankMesh> _fuelTanks = new();

        public bool ViewportVisibility
        {
            get => viewportVisibility;
            protected set => SetProperty(ref viewportVisibility, value);
        }
        private bool viewportVisibility;


        
        public SelectedItemWrapperViewModel(
            IEventAggregator eventAggregator,
            ILogger logger,
            IMapper mapper,
            IGeometryRepository geometryRepository,
            ISelectionManager selectionManager,
            IViewportElementService viewportElementService,
            IMaterialService materialService,
            IIndentService indentService
            ) : base()
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _geometryRepository = geometryRepository ?? throw new ArgumentNullException(nameof(geometryRepository));
            _selectionManager = selectionManager ?? throw new ArgumentNullException(nameof(selectionManager));
            _viewportElementService = viewportElementService ?? throw new ArgumentNullException(nameof(viewportElementService));
            _materialService = materialService ?? throw new ArgumentNullException(nameof(materialService));
            _indentService = indentService ?? throw new ArgumentNullException(nameof(indentService));

            var instanceThread = Thread.CurrentThread;

            Task.Factory.StartNew(() =>
            {
                while (Dispatcher.FromThread(instanceThread) == null) Thread.Sleep(10);

                Dispatcher = Dispatcher.FromThread(instanceThread);

                ActionInViewModel(() =>
                {
                    EffectsManager =
                        new DefaultEffectsManager();

                    Camera =
                        new HelixToolkit.Wpf.SharpDX.PerspectiveCamera
                        {
                            Position = new Point3D(0.5, 5, 0),
                            NearPlaneDistance = 0.1f,
                            FarPlaneDistance = 100000,
                            UpDirection = new Vector3D(-1, 0.5, 0),
                            LookDirection = new Vector3D(-1, -2, -0)
                        };

                    CoordinateLines.AddRange(_viewportElementService.CreateViewportLines());
                    GridLines.Add(_viewportElementService.CreateViewportGrid());
                });
            });
        }

        private void Initialize()
        {
            CurrentItem = _selectionManager.GetSelectedItem();
            var material = _materialService.CreateMaterial(Colors.Silver, 0.3f);

            ActionInViewModel(() =>
            {
                switch (CurrentItem)
                {
                    case ProjectModel:
                        ViewportVisibility = false;
                        break;

                    case FuelTankModel:
                        ViewportVisibility = true;

                        if (_geometryRepository.TryGetGeometryById(CurrentItem.Id, out var geometry))
                        {
                            FuelTanks.Add(new FuelTankMesh(
                                material,
                                material,
                                geometry
                                ));
                        }
                        break;

                    case SensorModel:
                        ViewportVisibility = false;
                        break;

                    case BranchModel:
                        ViewportVisibility = true;

                        var branch = CurrentItem as BranchModel;

                        foreach (var tank in branch.FuelTanks)
                        {
                            if (_geometryRepository.TryGetGeometryById(tank.Id, out var tankGeometry))
                            {
                                FuelTanks.Add(new FuelTankMesh(
                                    material,
                                    material,
                                    tankGeometry
                                    ));

                                tank.Sensors.ToList().ForEach(sensor =>
                                {
                                    var fuelSensorMesh = new FuelSensorMesh(
                                        Guid.NewGuid().ToString(),
                                        sensor.Name,
                                        PhongMaterials.Green,
                                        new Vector3(sensor.UpPoint.X, sensor.UpPoint.Y, sensor.UpPoint.Z),
                                        new Vector3(sensor.DownPoint.X, sensor.DownPoint.Y, sensor.DownPoint.Z),
                                        _indentService,
                                        tank.Id
                                        );

                                    FuelSensors.Add(fuelSensorMesh);
                                });
                            }
                        }

                        break;

                    case ConfigurationModel:
                        ViewportVisibility = true;

                        var configuration = CurrentItem as ConfigurationModel;

                        foreach (var tank in configuration.FuelTanks)
                        {
                            if (_geometryRepository.TryGetGeometryById(tank.Id, out var tankGeometry))
                            {
                                FuelTanks.Add(new FuelTankMesh(
                                    material,
                                    material,
                                    tankGeometry
                                    ));

                                tank.Sensors.ToList().ForEach(sensor =>
                                {
                                    var fuelSensorMesh = new FuelSensorMesh(
                                        Guid.NewGuid().ToString(),
                                        sensor.Name,
                                        PhongMaterials.Green,
                                        new Vector3(sensor.UpPoint.X, sensor.UpPoint.Y, sensor.UpPoint.Z),
                                        new Vector3(sensor.DownPoint.X, sensor.DownPoint.Y, sensor.DownPoint.Z),
                                        _indentService,
                                        tank.Id
                                        );

                                    FuelSensors.Add(fuelSensorMesh);
                                });
                            }
                        }

                        break;
                }

                if (FuelTanks.Any())
                {
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

                    Viewport3Dx.LookAt(new Point3D(center.X, center.Y, center.Z), 20000, 500);
                }
            });
        }
        

        public void Viewport3DxOnLoaded(object sender, RoutedEventArgs e)
        {
            Viewport3Dx = (Viewport3DX)sender;
            Initialize();
        }

        public void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            Window = (Window)sender;
        }

        private Point3D _holdCameraPosition;

        public void CameraChanged(object sender, RoutedEventArgs e)
        {
            _holdCameraPosition = Camera.Position;
        }

        public void ActionInViewModel(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            Dispatcher?.Invoke(priority,
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
            await Dispatcher?.BeginInvoke(priority,
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
               
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SelectedItemWrapperViewModel()
        {
            Dispose(false);
        }
    }
}
