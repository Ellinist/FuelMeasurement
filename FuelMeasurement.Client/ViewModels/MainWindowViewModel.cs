using DryIoc;
using FuelMeasurement.Client.Views;
using FuelMeasurement.Common.Constants;
using FuelMeasurement.Common.Events.SplashEvents;
using NLog;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Linq;
using FuelMeasurement.Common.Events.SelectionEvents;
using FuelMeasurement.Data.Repositories.Repositories.Interfaces;
using Prism.Services.Dialogs;

namespace FuelMeasurement.Client.ViewModels
{
    internal class MainWindowViewModel : BindableBase, IDisposable, IActionInViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly IDialogService _dialogService;
        private readonly IContainer _container;
        private readonly IRegionManager _regionManager;
        private readonly IGeometryRepository _geometryRepository;

        private readonly SubscriptionToken _closeViewToken;
        private readonly SubscriptionToken _selectionChangedToken;
        private bool _disposed = false;

        private readonly Dispatcher _dispatcher;

        private string _title = "Расстановка датчиков";

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private bool _activateTrigger = false;
        public bool ActivateTrigger
        {
            get => _activateTrigger;
            set => SetProperty(ref _activateTrigger, value);
        }

        public MainWindowViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public MainWindowViewModel(
            IEventAggregator eventAggregator, 
            ILogger logger,
            IDialogService dialogService,
            IContainer container,
            IRegionManager regionManager,
            IGeometryRepository geometryRepository
            ) : this()
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _geometryRepository = geometryRepository ?? throw new ArgumentNullException(nameof(geometryRepository));

            _closeViewToken = _eventAggregator.GetEvent<CloseSplashEvent>().Subscribe(OnCloseSplash, false);
            _selectionChangedToken = _eventAggregator.GetEvent<TreeViewSelectionChangedEvent>().Subscribe(OnSelectedItemChanged);
            
            InitializeRegions();
            InitializeCommand();
        }

        private void OnSelectedItemChanged(bool isVisible)
        {
            var region = _regionManager.Regions[RegionNames.WorkingRegion];

            var treeviewView = _container.Resolve<SelectedItemWrapper>();

            if (region.Views.Any())
            {
                region.RemoveAll();
            }

            if(isVisible)
            {
                region.Add(treeviewView);
            }
        }

        private void InitializeRegions()
        {
            ActionInViewModel(() => 
            {
                var region = _regionManager.Regions[RegionNames.TreeViewRegion];
                if (region != null)
                {
                    var treeviewView = _container.Resolve<TreeviewView>();
                    region.Add(treeviewView);
                }
            });
        }

        private void InitializeCommand()
        {
            
        }

        private void OnCloseSplash()
        {
            if (!ActivateTrigger)
            {
                ActivateTrigger = true;
                _closeViewToken?.Dispose();
            }
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

        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _closeViewToken?.Dispose();
                _selectionChangedToken?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MainWindowViewModel()
        {
            Dispose(false);
        }
    }
}
