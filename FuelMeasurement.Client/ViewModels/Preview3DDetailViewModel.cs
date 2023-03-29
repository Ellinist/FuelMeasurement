using FuelMeasurement.Client.UIModule.Models;
using FuelMeasurement.Client.UIModule.Services;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using NLog;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace FuelMeasurement.Client.ViewModels
{
    internal class Preview3DDetailViewModel : BindableBase, IDisposable, IActionInViewModel
    {
        private readonly Dispatcher _dispatcher;
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;

        private bool _disposed = false;

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

        private string _selectedElementPreviewElementHeader = String.Empty;
        public string SelectedElementPreviewElementHeader
        {
            get => _selectedElementPreviewElementHeader;
            set => SetProperty(ref _selectedElementPreviewElementHeader, value);
        }

        public Window Window { get; set; }

        public ObservableCollection<FuelTankMesh> FuelTanks
        {
            get => _fuelTanks;
            protected set => SetProperty(ref _fuelTanks, value);
        }
        private ObservableCollection<FuelTankMesh> _fuelTanks = new();

        public Preview3DDetailViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public Preview3DDetailViewModel
            (
            IEventAggregator eventAggregator,
            ILogger logger
            ) : this()
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SelectedElementPreviewElementHeader = "Что то выбрано в тривью!";

            InitializeCommand();

            ActionInViewModel(() =>
            {
                //EffectsManager =
                //    new DefaultEffectsManager();

                Camera =
                    new HelixToolkit.Wpf.SharpDX.PerspectiveCamera
                    {
                        Position = new Point3D(0.5, 5, 0),
                        NearPlaneDistance = 0.1f,
                        FarPlaneDistance = 100000,
                        UpDirection = new Vector3D(-1, 0.5, 0),
                        LookDirection = new Vector3D(-1, -2, -0)
                    };

                var center = new Vector3D(0, 0, 0);

                Viewport3Dx.LookAt(new Point3D(center.X, center.Y, center.Z), 20000, 500);
            });
        }

        private void InitializeCommand()
        {
            
        }

        public void Viewport3DxOnLoaded(object sender, RoutedEventArgs e)
        {
            Viewport3Dx = (Viewport3DX)sender;
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

        ~Preview3DDetailViewModel()
        {
            Dispose(false);
        }
    }
}
